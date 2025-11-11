using EVehicle.Application;
using EVehicle.Infrastructure;
using EVehicle.API.Middleware;
using EVehicle.API.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EVehicle API",
        Version = "v1",
        Description = "API cho hệ thống Sàn giao dịch C2C xe điện và pin",
        Contact = new OpenApiContact
        {
            Name = "EVehicle Team",
            Email = "support@evehicle.vn"
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Cấu hình JWT trong Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme. \r\n\r\n" +
                      "Nhập 'Bearer' [space] và sau đó nhập token của bạn.\r\n\r\n" +
                      "Ví dụ: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Group endpoints by tags
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
    options.DocInclusionPredicate((name, api) => true);
});

// Configure Infrastructure (DbContext, Repositories, Infrastructure Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Application (Business Logic, Application Services, Validators)
builder.Services.AddApplicationServices();

// Configure File Storage Service (cần IWebHostEnvironment từ API layer)
// Note: IFileStorageOptions được cấu hình ở đây vì chỉ có API layer mới có thể truy cập IWebHostEnvironment
builder.Services.AddScoped<EVehicle.Application.Interfaces.IFileStorageOptions>(serviceProvider =>
{
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var rootPath = environment.WebRootPath;
    if (string.IsNullOrEmpty(rootPath))
    {
        // Tạo wwwroot folder nếu chưa có
        rootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
    }
    return new EVehicle.Infrastructure.Services.FileStorageOptions(rootPath);
});
builder.Services.AddScoped<EVehicle.Application.Interfaces.IFileStorageService, EVehicle.Infrastructure.Services.FileStorageService>();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Jwt:SecretKey chưa được cấu hình");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EVehicleAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EVehicleClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero,
        // Map claims từ JWT token vào ClaimsPrincipal
        // NameClaimType và RoleClaimType được dùng để map claims vào User.Identity.Name và User.IsInRole()
        // QUAN TRỌNG: RoleClaimType phải khớp với claim type trong token
        // - Token có claim type "role", nên set RoleClaimType = "role"
        // - Middleware sẽ tìm claim type "role" trong token và map vào ClaimsPrincipal
        // - Nhưng trong .NET, nó sẽ tự động map vào ClaimTypes.Role trong ClaimsPrincipal
        // - Policy sử dụng RequireClaim(ClaimTypes.Role, "MEMBER") để kiểm tra
        NameClaimType = "userId", // Sử dụng "userId" làm name claim
        RoleClaimType = "role" // Token có claim type "role", middleware sẽ map nó vào ClaimTypes.Role trong ClaimsPrincipal
    };

    // Event handler để debug và log khi có vấn đề với authentication
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            logger.LogError(context.Exception, 
                "JWT Authentication failed. Path: {Path}, HasAuthHeader: {HasAuthHeader}, AuthHeaderLength: {AuthHeaderLength}",
                context.Request.Path,
                !string.IsNullOrEmpty(authHeader),
                authHeader?.Length ?? 0);
            
            // Log một phần của token để debug (không log toàn bộ vì security)
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring(7).Trim();
                logger.LogError("Token preview (first 20 chars): {TokenPreview}...", 
                    token.Length > 20 ? token.Substring(0, 20) : token);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("userId")?.Value;
            var role = context.Principal?.FindFirst("role")?.Value;
            var roleFromClaimTypes = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var allClaims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList() ?? new List<string>();
            
            logger.LogInformation("JWT Token validated successfully. UserId: {UserId}, Role (from 'role'): {Role}, Role (from ClaimTypes.Role): {RoleFromClaimTypes}, Path: {Path}, ClaimsCount: {ClaimsCount}",
                userId, role ?? "null", roleFromClaimTypes ?? "null", context.Request.Path, allClaims.Count);
            logger.LogInformation("All claims in token: {Claims}", string.Join(", ", allClaims));
            
            // QUAN TRỌNG: Đảm bảo claim "role" tồn tại với type "role" (RoleClaimType)
            // JWT Bearer middleware có thể map claim "role" thành ClaimTypes.Role,
            // nhưng để authorization policy hoạt động đúng, cần có cả hai claims
            if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity claimsIdentity)
            {
                // Lấy role value từ bất kỳ claim nào (có thể là "role" hoặc ClaimTypes.Role)
                var roleValue = role ?? roleFromClaimTypes;
                
                if (!string.IsNullOrEmpty(roleValue))
                {
                    // Đảm bảo có claim với type "role" (custom type)
                    var existingRoleClaim = claimsIdentity.FindFirst("role");
                    if (existingRoleClaim == null)
                    {
                        // Nếu chưa có, thêm claim với type "role"
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim("role", roleValue));
                        logger.LogInformation("Added 'role' claim with value: {Role}", roleValue);
                    }
                    
                    // Đảm bảo có claim với type ClaimTypes.Role
                    var existingClaimTypesRole = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Role);
                    if (existingClaimTypesRole == null)
                    {
                        // Nếu chưa có, thêm claim với type ClaimTypes.Role
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleValue));
                        logger.LogInformation("Added ClaimTypes.Role claim with value: {Role}", roleValue);
                    }
                    
                    // Lưu ý: RoleClaimType đã được set trong JWT Bearer configuration (dòng 128)
                    // và là read-only property, không thể thay đổi tại đây
                }
                
                // Kiểm tra xem User.IsInRole có hoạt động không
                var isInRoleMEMBER = context.Principal?.IsInRole("MEMBER") ?? false;
                var isInRoleSTAFF = context.Principal?.IsInRole("STAFF") ?? false;
                var isInRoleADMIN = context.Principal?.IsInRole("ADMIN") ?? false;
                var roleClaimType = claimsIdentity.RoleClaimType;
                logger.LogInformation("User.IsInRole('MEMBER'): {IsInRoleMEMBER}, User.IsInRole('STAFF'): {IsInRoleSTAFF}, User.IsInRole('ADMIN'): {IsInRoleADMIN}, RoleClaimType: {RoleClaimType}", 
                    isInRoleMEMBER, isInRoleSTAFF, isInRoleADMIN, roleClaimType);
            }
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated ?? false;
            var claimsCount = context.HttpContext.User?.Claims?.Count() ?? 0;
            var userId = context.HttpContext.User?.FindFirst("userId")?.Value;
            var role = context.HttpContext.User?.FindFirst("role")?.Value;
            
            logger.LogWarning(
                "JWT Authentication challenge. Path: {Path}, Error: {Error}, ErrorDescription: {ErrorDescription}, " +
                "HasAuthHeader: {HasAuthHeader}, IsAuthenticated: {IsAuthenticated}, ClaimsCount: {ClaimsCount}, " +
                "UserId: {UserId}, Role: {Role}",
                context.Request.Path,
                context.Error ?? "null",
                context.ErrorDescription ?? "null",
                !string.IsNullOrEmpty(authHeader),
                isAuthenticated,
                claimsCount,
                userId ?? "null",
                role ?? "null");
            
            // Log tất cả claims nếu có
            if (context.HttpContext.User?.Claims != null && context.HttpContext.User.Claims.Any())
            {
                var allClaims = context.HttpContext.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                logger.LogWarning("All claims in challenge: {Claims}", string.Join(", ", allClaims));
            }
            
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Skip logging cho static files, test UI và login page
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || 
                path.StartsWith("/test-ui") || 
                path.StartsWith("/ui") ||
                path.StartsWith("/login") ||
                path.EndsWith(".html") || 
                path.EndsWith(".css") || 
                path.EndsWith(".js") || 
                path.EndsWith(".png") || 
                path.EndsWith(".jpg") || 
                path.EndsWith(".jpeg") || 
                path.EndsWith(".gif") ||
                path == "/")
            {
                return Task.CompletedTask;
            }

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            logger.LogInformation("JWT Message received. Path: {Path}, HasAuthHeader: {HasAuthHeader}, HeaderLength: {HeaderLength}",
                context.Request.Path,
                !string.IsNullOrEmpty(authHeader),
                authHeader?.Length ?? 0);
            
            if (!string.IsNullOrEmpty(authHeader))
            {
                // Log first 50 chars of header để debug (không log toàn bộ vì security)
                var headerPreview = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader;
                logger.LogInformation("Authorization header preview: {HeaderPreview}", headerPreview);
                
                // Normalize header: trim và check case-insensitive
                var normalizedHeader = authHeader.Trim();
                
                if (normalizedHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = normalizedHeader.Substring(7).Trim();
                    logger.LogInformation("Token extracted. Length: {TokenLength}, Preview: {TokenPreview}...",
                        token.Length,
                        token.Length > 30 ? token.Substring(0, 30) : token);
                    
                    // Set token vào context để JWT Bearer middleware xử lý
                    context.Token = token;
                    
                    // Try to decode token để kiểm tra format
                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        if (handler.CanReadToken(token))
                        {
                            var jsonToken = handler.ReadJwtToken(token);
                            logger.LogInformation("Token decoded successfully. Issuer: {Issuer}, Audience: {Audience}, Expires: {Expires}",
                                jsonToken.Issuer,
                                string.Join(",", jsonToken.Audiences),
                                jsonToken.ValidTo);
                        }
                        else
                        {
                            logger.LogWarning("Token cannot be read by JWT handler");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error decoding token");
                    }
                }
                else
                {
                    // Log chi tiết để debug
                    logger.LogWarning(
                        "Authorization header does not start with 'Bearer '. " +
                        "Header starts with: '{HeaderStart}' (length: {Length})",
                        normalizedHeader.Length > 10 ? normalizedHeader.Substring(0, 10) : normalizedHeader,
                        normalizedHeader.Length);
                    
                    // Thử xử lý trường hợp token được gửi không có "Bearer " prefix
                    // (một số client có thể gửi token trực tiếp)
                    if (!string.IsNullOrWhiteSpace(normalizedHeader))
                    {
                        // Giả sử toàn bộ header là token (nếu không có "Bearer ")
                        logger.LogInformation("Attempting to use entire header as token (no Bearer prefix)");
                        context.Token = normalizedHeader;
                    }
                }
            }
            else
            {
                logger.LogWarning("Authorization header is empty or null");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Cấu hình authorization policy để sử dụng role từ JWT token
    // Sử dụng RequireClaim với claim type "role" (custom claim type) thay vì ClaimTypes.Role
    // Vì JWT token sử dụng claim type "role" và RoleClaimType = "role"
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    // Helper function để tạo policy cho multiple roles
    void AddRolePolicy(string policyName, params string[] roles)
    {
        if (roles == null || roles.Length == 0)
            return;

        if (roles.Length == 1)
        {
            // Single role: sử dụng RequireAssertion để kiểm tra cả "role" và ClaimTypes.Role
            var requiredRole = roles[0];
            options.AddPolicy(policyName, policy => 
                policy.RequireAssertion(context =>
                {
                    // Kiểm tra tất cả các claim types có thể chứa role
                    // Tìm trong tất cả claims để đảm bảo tìm được role dù claim type là gì
                    var roleClaim = context.User.Claims
                        .FirstOrDefault(c => 
                            (c.Type == "role" || 
                             c.Type == System.Security.Claims.ClaimTypes.Role ||
                             c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)) &&
                            !string.IsNullOrEmpty(c.Value));
                    
                    var userRole = roleClaim?.Value;
                    
                    // Kiểm tra authorization
                    var isAuthorized = !string.IsNullOrEmpty(userRole) && 
                                      string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase);
                    
                    return isAuthorized;
                }));
        }
        else
        {
            // Multiple roles: sử dụng RequireAssertion
            var allowedRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            options.AddPolicy(policyName, policy => 
                policy.RequireAssertion(context =>
                {
                    // Kiểm tra tất cả các claim types có thể chứa role
                    // Tìm trong tất cả claims để đảm bảo tìm được role dù claim type là gì
                    var roleClaim = context.User.Claims
                        .FirstOrDefault(c => 
                            (c.Type == "role" || 
                             c.Type == System.Security.Claims.ClaimTypes.Role ||
                             c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)) &&
                            !string.IsNullOrEmpty(c.Value));
                    
                    var userRole = roleClaim?.Value;
                    
                    // Kiểm tra authorization
                    var isAuthorized = !string.IsNullOrEmpty(userRole) && allowedRoles.Contains(userRole);
                    
                    return isAuthorized;
                }));
        }
    }
    
    AddRolePolicy("MEMBER", "MEMBER");
    AddRolePolicy("STAFF", "STAFF");
    AddRolePolicy("ADMIN", "ADMIN");
    AddRolePolicy("ADMIN_MEMBER_STAFF", "ADMIN", "MEMBER", "STAFF");
    AddRolePolicy("MEMBER_STAFF_ADMIN", "ADMIN", "MEMBER", "STAFF"); // Alias (same roles, different order)
    AddRolePolicy("ADMIN_STAFF", "ADMIN", "STAFF");
    AddRolePolicy("MEMBER_STAFF", "MEMBER", "STAFF");
    AddRolePolicy("ADMIN_MEMBER", "ADMIN", "MEMBER");
});

// Custom Authorization Policy Provider để tạo policy động cho multiple roles
// Đăng ký sau khi AddAuthorization để có thể override policy provider
builder.Services.AddSingleton<IAuthorizationPolicyProvider, RoleBasedPolicyProvider>();

// Configure CORS - Cho phép tất cả origins, methods, và headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Cho phép tất cả origins (tương đương AllowAnyOrigin nhưng linh hoạt hơn)
                  .AllowAnyMethod()              // Cho phép tất cả HTTP methods (GET, POST, PUT, DELETE, etc.)
                  .AllowAnyHeader()              // Cho phép tất cả headers
                  .AllowCredentials();           // Cho phép credentials (cookies, authorization headers)
        });
});

var app = builder.Build();

// Cấu hình để lắng nghe trên tất cả network interfaces (0.0.0.0) để có thể truy cập từ máy khác
// Đọc port từ configuration hoặc sử dụng port mặc định
var kestrelHttpUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"];
var urlsConfig = builder.Configuration["Urls"];
var httpPort = "9190"; // Default port

// Parse port từ Kestrel configuration (format: http://0.0.0.0:9290)
if (!string.IsNullOrEmpty(kestrelHttpUrl))
{
    var uri = new Uri(kestrelHttpUrl);
    httpPort = uri.Port.ToString();
}
// Parse port từ Urls configuration (format: http://0.0.0.0:9290)
else if (!string.IsNullOrEmpty(urlsConfig))
{
    var firstUrl = urlsConfig.Split(';').FirstOrDefault();
    if (!string.IsNullOrEmpty(firstUrl) && Uri.TryCreate(firstUrl, UriKind.Absolute, out var uri))
    {
        httpPort = uri.Port.ToString();
    }
}
// Fallback to environment variable hoặc default
else
{
    httpPort = builder.Configuration["ASPNETCORE_HTTP_PORTS"] 
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")
        ?? "9190";
}

var httpsPort = builder.Configuration["Kestrel:Endpoints:Https:Url"]?.Split(':').LastOrDefault() 
    ?? builder.Configuration["ASPNETCORE_HTTPS_PORTS"] 
    ?? "9191";

// Kiểm tra xem có muốn bind trên public IP không (mặc định là true)
var bindToPublicIp = builder.Configuration.GetValue<bool>("BindToPublicIp", true);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (bindToPublicIp)
{
    // Cấu hình URLs để lắng nghe trên tất cả interfaces (0.0.0.0)
    // Cho phép truy cập từ máy khác thông qua public IP hoặc local network IP
    app.Urls.Clear();
    
    // HTTP: luôn bind trên 0.0.0.0 để có thể truy cập từ máy khác
    app.Urls.Add($"http://0.0.0.0:{httpPort}");
    logger.LogInformation("HTTP đang lắng nghe trên http://0.0.0.0:{HttpPort} (có thể truy cập từ http://<public-ip>:{HttpPort})", httpPort, httpPort);
    
    // HTTPS: đã tắt - chỉ sử dụng HTTP
    // app.Urls.Add($"https://0.0.0.0:{httpsPort}");
    
    logger.LogInformation("Lưu ý: Thay <public-ip> bằng địa chỉ IP public hoặc local network IP của máy này để truy cập từ máy khác");
    logger.LogInformation("Để tắt binding trên public IP, set 'BindToPublicIp': false trong appsettings.json");
}
else
{
    logger.LogInformation("Binding chỉ trên localhost (BindToPublicIp = false)");
}

// Seed database (chỉ chạy trong Development)
if (app.Environment.IsDevelopment())
{
    try
    {
        await app.Services.SeedDatabaseAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi seed database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EVehicle API V1");
    });
}

// HTTPS Redirection đã tắt - chỉ sử dụng HTTP
// app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Enable static files for uploaded images and test UI
app.UseStaticFiles();

// Authentication & Authorization middleware (thứ tự quan trọng)
app.UseAuthentication();
app.UseAuthorization();

// Authentication Debug Middleware (sau Authentication để debug)
app.UseAuthenticationDebug();

// Role Validation Middleware (sau Authentication để có thể đọc JWT token)
app.UseRoleValidation();

app.MapControllers();

// Redirect routes for test UI and login (sau MapControllers để không conflict)
// Note: Static files (index.html) sẽ được serve trước, nên các route này chỉ hoạt động nếu không có file static match
app.MapGet("/test-ui", () => Results.Redirect("/test-ui.html"));
app.MapGet("/ui", () => Results.Redirect("/test-ui.html"));
app.MapGet("/login", () => Results.Redirect("/login.html"));

app.Run();

