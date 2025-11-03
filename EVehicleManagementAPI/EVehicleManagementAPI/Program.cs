using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Services;
using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EVehicleManagementAPI", Version = "v1" });

    // JWT Authorize button
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "Nhập token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new List<string>()
        }
    });
});

// HttpClient & DI services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// Configure DbContext
builder.Services.AddDbContext<EVehicleDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)
    );
    // Chỉ cảnh báo khi có pending model changes (không throw) để cho phép auto-migrate tiếp tục
    options.ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_dev_dev_change_me_in_production_at_least_32_characters";

// Validate JWT key length
if (jwtKey.Length < 32)
{
    Console.WriteLine("⚠️ WARNING: JWT Key is too short (minimum 32 characters). Using fallback key.");
    jwtKey = "dev_dev_dev_change_me_in_production_at_least_32_characters_secure_key_2024";
}

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
        IssuerSigningKey = signingKey,
        // ✅ Validate token expiration
        ValidateLifetime = true,
        // ✅ Cho phép clock skew (chênh lệch thời gian giữa server)
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    
    // ✅ Xử lý events để log errors
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"⚠️ JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Token hợp lệ - có thể thêm custom logic ở đây
            return Task.CompletedTask;
        }
    };
});

Console.WriteLine($"✅ JWT Authentication configured (Key length: {jwtKey.Length} characters)");

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

// ✅ Serve static files (wwwroot) để truy cập ảnh qua URL
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Quick health endpoint
app.MapGet("/", () => Results.Text("App running"));

app.MapControllers();

// ✅ Auto-migrate database on startup (safe - only adds missing tables/columns)
// ✅ Seed default data (idempotent - chỉ tạo nếu chưa có)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EVehicleDbContext>();
    // ✅ Đảm bảo thư mục uploads tồn tại
    try
    {
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(webRoot);
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "vehicle-models"));
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "battery-models"));
    }
    catch { }
    try
    {
        // Tuỳ chọn: Drop & Recreate toàn bộ schema nếu bật cờ cấu hình (chỉ dùng khi DB chưa có dữ liệu)
        var dropAndRecreate = builder.Configuration.GetValue<bool>("Database:DropAndRecreate");
        if (dropAndRecreate)
        {
            Console.WriteLine("⚠️ Database:DropAndRecreate=true → Dropping and recreating database schema...");
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            Console.WriteLine("✅ Database dropped and recreated successfully");
        }
        else
        {
            // Apply migrations bình thường
            db.Database.Migrate();
            Console.WriteLine("✅ Database migrations applied successfully");
        }
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

// ✅ Seed default data: roles, accounts, and test data (idempotent)
static async Task SeedDefaultData(EVehicleDbContext db)
{
    // ========== SEED ROLES ==========
    var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole == null)
    {
        adminRole = new Role { Name = "Admin", Status = "ACTIVE" };
        db.Roles.Add(adminRole);
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Admin role created");
    }

    var staffRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
    if (staffRole == null)
    {
        staffRole = new Role { Name = "Staff", Status = "ACTIVE" };
        db.Roles.Add(staffRole);
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Staff role created");
    }

    var memberRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
    if (memberRole == null)
    {
        memberRole = new Role { Name = "Member", Status = "ACTIVE" };
        db.Roles.Add(memberRole);
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Member role created");
    }

    // ========== SEED DEFAULT ACCOUNTS ==========
    
    // Admin Account
    var adminEmail = "admin@demo.com";
    if (!await db.Accounts.AnyAsync(a => a.Email == adminEmail))
    {
        var adminAccount = new Account
        {
            Email = adminEmail,
            PasswordHash = HashPassword("Admin@123"),
            RoleId = adminRole.RoleId,
            Phone = "0901234567",
            EmailVerified = true,
            CreatedAt = DateTime.Now
        };
        db.Accounts.Add(adminAccount);
        await db.SaveChangesAsync();
        Console.WriteLine($"✅ Admin account: {adminEmail} / Admin@123");
    }

    // Staff Account
    var staffEmail = "staff@demo.com";
    if (!await db.Accounts.AnyAsync(a => a.Email == staffEmail))
    {
        var staffAccount = new Account
        {
            Email = staffEmail,
            PasswordHash = HashPassword("Staff@123"),
            RoleId = staffRole.RoleId,
            Phone = "0901234568",
            EmailVerified = true,
            CreatedAt = DateTime.Now
        };
        db.Accounts.Add(staffAccount);
        await db.SaveChangesAsync();
        Console.WriteLine($"✅ Staff account: {staffEmail} / Staff@123");
    }

    // Test Member Accounts (3 accounts for testing)
    var testAccounts = new[]
    {
        new { Email = "user1@demo.com", Password = "User1@123", Name = "Nguyễn Văn A", Phone = "0901111111" },
        new { Email = "user2@demo.com", Password = "User2@123", Name = "Trần Thị B", Phone = "0902222222" },
        new { Email = "user3@demo.com", Password = "User3@123", Name = "Lê Văn C", Phone = "0903333333" }
    };

    foreach (var testAccount in testAccounts)
    {
        if (!await db.Accounts.AnyAsync(a => a.Email == testAccount.Email))
        {
            var account = new Account
            {
                Email = testAccount.Email,
                PasswordHash = HashPassword(testAccount.Password),
                RoleId = memberRole.RoleId,
                Phone = testAccount.Phone,
                EmailVerified = true, // ✅ Email verified cho tất cả test accounts
                CreatedAt = DateTime.Now
            };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            // Create Member record for each account
            var member = new Member
            {
                AccountId = account.AccountId,
                FullName = testAccount.Name,
                AvatarUrl = "",
                Address = "123 Test Street, Ho Chi Minh City",
                JoinedAt = DateTime.Now,
                Rating = 5.0m,
                Status = "ACTIVE"
            };
            db.Members.Add(member);
            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Test member: {testAccount.Email} / {testAccount.Password} - {testAccount.Name}");
        }
    }

    // ========== SEED POST PACKAGES ==========
    if (!await db.PostPackages.AnyAsync())
    {
        var packages = new[]
        {
            new PostPackage
            {
                Name = "Gói Cơ Bản",
                DurationDay = 7,
                Price = 50000,
                PriorityLevel = 1,
                Description = "Gói cơ bản - Hiển thị 7 ngày"
            },
            new PostPackage
            {
                Name = "Gói Tiêu Chuẩn",
                DurationDay = 14,
                Price = 90000,
                PriorityLevel = 2,
                Description = "Gói tiêu chuẩn - Hiển thị 14 ngày, ưu tiên cao hơn"
            },
            new PostPackage
            {
                Name = "Gói Premium",
                DurationDay = 30,
                Price = 180000,
                PriorityLevel = 3,
                Description = "Gói premium - Hiển thị 30 ngày, ưu tiên cao nhất, featured"
            }
        };

        foreach (var package in packages)
        {
            db.PostPackages.Add(package);
        }
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Post packages created (3 packages)");
    }

    // ========== SEED VEHICLE MODELS (Sample) ==========
    if (!await db.VehicleModels.AnyAsync())
    {
        var vehicleModels = new[]
        {
            new VehicleModel
            {
                Name = "VinFast VF e34",
                Brand = "VinFast",
                Year = 2021,
                Type = "SUV",
                MotorPower = 110,
                BatteryType = "LFP",
                Voltage = 400,
                Range = 285,
                Weight = 1650,
                Seats = 5,
                Description = "Mẫu xe điện SUV đầu tiên của VinFast",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            },
            new VehicleModel
            {
                Name = "Tesla Model 3",
                Brand = "Tesla",
                Year = 2023,
                Type = "Sedan",
                MotorPower = 283,
                BatteryType = "NMC",
                Voltage = 350,
                Range = 547,
                Weight = 1847,
                Seats = 5,
                Description = "Xe điện sedan phổ biến của Tesla",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            },
            new VehicleModel
            {
                Name = "PEGA CITY",
                Brand = "PEGA",
                Year = 2022,
                Type = "E-Bike",
                MotorPower = 1.5m,
                BatteryType = "Li-ion",
                Voltage = 48,
                Range = 60,
                Weight = 75,
                Seats = 1,
                Description = "Xe đạp điện đô thị",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            }
        };

        foreach (var model in vehicleModels)
        {
            db.VehicleModels.Add(model);
        }
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Vehicle models created (3 models)");
    }

    // ========== SEED BATTERY MODELS (Sample) ==========
    if (!await db.BatteryModels.AnyAsync())
    {
        var batteryModels = new[]
        {
            new BatteryModel
            {
                Name = "Lithium-ion 48V 20Ah",
                Brand = "LG",
                Chemistry = "Li-ion",
                Voltage = 48,
                CapacityKWh = 0.96m,
                Amperage = 20,
                FormFactor = "Rectangular",
                Weight = 8.5m,
                Cycles = 2000,
                Description = "Pin lithium-ion 48V phổ biến cho xe đạp điện",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            },
            new BatteryModel
            {
                Name = "NMC 400V 60kWh",
                Brand = "CATL",
                Chemistry = "NMC",
                Voltage = 400,
                CapacityKWh = 60,
                Amperage = 150,
                FormFactor = "Pouch",
                Weight = 380,
                Cycles = 1500,
                Description = "Pin NMC công suất cao cho xe điện",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            },
            new BatteryModel
            {
                Name = "LFP 51.2V 100Ah",
                Brand = "BYD",
                Chemistry = "LFP",
                Voltage = 51.2m,
                CapacityKWh = 5.12m,
                Amperage = 100,
                FormFactor = "Prismatic",
                Weight = 55,
                Cycles = 3000,
                Description = "Pin LFP an toàn, tuổi thọ cao",
                IsCustom = false,
                IsApproved = true,
                CreatedAt = DateTime.Now
            }
        };

        foreach (var model in batteryModels)
        {
            db.BatteryModels.Add(model);
        }
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Battery models created (3 models)");
    }

    Console.WriteLine("✅ All default data seeded successfully!");
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
