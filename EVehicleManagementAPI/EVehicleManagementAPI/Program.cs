using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EVehicleDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)
    )
);

// ✅ Thêm cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // Hỗ trợ cả HTTP và HTTPS cho frontend
            policy.WithOrigins(
                "http://localhost:5173",   // HTTP frontend
                "https://localhost:5173"    // HTTPS frontend (nếu có)
            )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // chỉ thêm nếu bạn dùng cookie/token
        });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient & DI services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_dev_dev_change_me";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey
    };
});

var app = builder.Build();

// show detailed errors in Development to diagnose startup exceptions
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Thêm dòng này TRƯỚC khi map controller
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// quick health endpoint so you can confirm app started
app.MapGet("/", () => Results.Text("App running"));

app.MapControllers();

// Seed Staff role and a default Staff account on startup (idempotent)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EVehicleDbContext>();

    // Ensure database exists/migrated
    // ⚠️ CHỈ dùng Migrate(), KHÔNG dùng EnsureCreated() vì:
    // - EnsureCreated() không chạy migrations và có thể xóa/ghi đè dữ liệu
    // - EnsureCreated() không tương thích với migration system
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log error nhưng không crash app (cho phép chạy trong môi trường không có DB permission)
        // Trong production nên log lỗi này
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Migration error (non-fatal): {ex.Message}");
        }
    }

    // Seeding: Tạo các default roles nếu chưa có (idempotent - an toàn với dữ liệu hiện có)
    // Đảm bảo có đủ 3 roles cơ bản: Admin, Member, Staff (theo yêu cầu của team member)
    var rolesToSeed = new[]
    {
        new { Name = "Admin", Status = "ACTIVE" },
        new { Name = "Member", Status = "ACTIVE" },
        new { Name = "Staff", Status = "ACTIVE" }
    };

    foreach (var roleInfo in rolesToSeed)
    {
        var existingRole = db.Roles.FirstOrDefault(r => r.Name == roleInfo.Name);
        if (existingRole == null)
        {
            var newRole = new EVehicleManagementAPI.Models.Role 
            { 
                Name = roleInfo.Name, 
                Status = roleInfo.Status 
            };
            db.Roles.Add(newRole);
        }
    }
    db.SaveChanges(); // Save một lần sau khi check tất cả roles

    // Get Staff role để dùng cho staff account (sau khi đã seed)
    var staffRole = db.Roles.FirstOrDefault(r => r.Name == "Staff");

    // Default staff account (chỉ tạo nếu Staff role đã tồn tại)
    if (staffRole != null)
    {
    var staffEmail = "staff@demo.com";
    var existingStaff = db.Accounts.Include(a => a.Role).FirstOrDefault(a => a.Email == staffEmail);
    if (existingStaff == null)
    {
        string Hash(string s)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
            return Convert.ToBase64String(bytes);
        }

        var account = new EVehicleManagementAPI.Models.Account
        {
            Email = staffEmail,
            Phone = "",
            PasswordHash = Hash("Staff@123"),
            RoleId = staffRole.RoleId,
            CreatedAt = DateTime.Now
        };
        db.Accounts.Add(account);
        db.SaveChanges();
        }
    }
}
app.Run();
