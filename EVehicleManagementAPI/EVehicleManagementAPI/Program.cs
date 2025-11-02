using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Services;
using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services
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

// Configure DbContext
builder.Services.AddDbContext<EVehicleDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)
    ));

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_dev_dev_change_me_in_production_at_least_32_characters";
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

// ✅ Thêm cấu hình CORS cho frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // Lấy allowed origins từ configuration hoặc environment variable
            var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"]?.Split(';') 
                ?? new[] { "http://localhost:5173", "https://localhost:5173", "http://localhost:3000", "https://localhost:3000" };
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Kích hoạt CORS trước khi dùng Authorization
app.UseCors("AllowFrontend");

// Chỉ redirect HTTPS trong production, development cho phép cả HTTP và HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Quick health endpoint
app.MapGet("/", () => Results.Text("App running"));

app.MapControllers();

// ✅ Auto-migrate database on startup (safe - only adds missing tables/columns)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EVehicleDbContext>();
    try
    {
        // Chỉ apply migrations chưa được apply (an toàn - không ghi đè dữ liệu)
        db.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration error: {ex.Message}");
        // Không throw để app vẫn có thể chạy nếu migration fail
    }

    // ✅ Seed default data (idempotent - chỉ tạo nếu chưa có)
    try
    {
        await SeedDefaultData(db);
        Console.WriteLine("✅ Default data seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Seeding error: {ex.Message}");
    }
}

app.Run();

// ✅ Seed default roles and staff account (idempotent)
static async Task SeedDefaultData(EVehicleDbContext db)
{
    // Seed Roles
    var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole == null)
    {
        adminRole = new Role { Name = "Admin", Description = "Administrator role", Status = "ACTIVE" };
        db.Roles.Add(adminRole);
        await db.SaveChangesAsync();
    }

    var staffRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
    if (staffRole == null)
    {
        staffRole = new Role { Name = "Staff", Description = "Staff role", Status = "ACTIVE" };
        db.Roles.Add(staffRole);
        await db.SaveChangesAsync();
    }

    var memberRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
    if (memberRole == null)
    {
        memberRole = new Role { Name = "Member", Description = "Member role", Status = "ACTIVE" };
        db.Roles.Add(memberRole);
        await db.SaveChangesAsync();
    }

    // Seed default staff account (only if not exists)
    var defaultStaffEmail = "staff@demo.com";
    var defaultStaffExists = await db.Accounts.AnyAsync(a => a.Email == defaultStaffEmail);
    if (!defaultStaffExists)
    {
        var hashedPassword = HashPassword("Staff@123"); // Default password
        var defaultStaff = new Account
        {
            Email = defaultStaffEmail,
            PasswordHash = hashedPassword,
            RoleId = staffRole.RoleId,
            Phone = "0123456789",
            CreatedAt = DateTime.Now
        };
        db.Accounts.Add(defaultStaff);
        await db.SaveChangesAsync();
        Console.WriteLine($"✅ Default staff account created: {defaultStaffEmail} / Staff@123");
    }
}

// Password hashing helper (same as AuthController)
static string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
