using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVehicle.Infrastructure.Data;

/// <summary>
/// Service để seed dữ liệu ban đầu cho database
/// </summary>
public class DbSeeder
{
    private readonly EVehicleDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(EVehicleDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed tất cả dữ liệu ban đầu
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Bắt đầu seed dữ liệu...");

            // Seed Categories trước (cần cho ContractTemplates)
            await SeedCategoriesAsync();
            
            // Seed Package Definitions
            await SeedPackageDefinitionsAsync();
            
            // Seed Contract Templates (cần Categories)
            await SeedContractTemplatesAsync();
            
            // Seed Admin User
            await SeedAdminUserAsync();
            
            // Seed Fake Users (cần cho Posts)
            await SeedFakeUsersAsync();
            
            // Seed Posts (cần Users và Categories)
            await SeedPostsAsync();

            // Save tất cả thay đổi còn lại
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seed dữ liệu hoàn tất!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi seed dữ liệu");
            throw;
        }
    }

    /// <summary>
    /// Seed Categories (Xe điện, Pin)
    /// </summary>
    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories đã tồn tại, bỏ qua seed.");
            return;
        }

        var categories = new List<Category>
        {
            new Category
            {
                Name = "Xe điện",
                Code = "ELECTRIC_VEHICLE",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Pin",
                Code = "BATTERY",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync(); // Save để có ID
        _logger.LogInformation("Đã seed {Count} categories", categories.Count);
    }

    /// <summary>
    /// Seed Package Definitions (Basic, Premium, Luxury)
    /// </summary>
    private async Task SeedPackageDefinitionsAsync()
    {
        if (await _context.PackageDefinitions.AnyAsync())
        {
            _logger.LogInformation("Package Definitions đã tồn tại, bỏ qua seed.");
            return;
        }

        var packages = new List<PackageDefinition>
        {
            new PackageDefinition
            {
                Name = "Basic",
                Price = 50000, // 50,000 VND
                CreditsCount = 20,
                PriorityLevel = 1,
                MaxImages = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new PackageDefinition
            {
                Name = "Premium",
                Price = 150000, // 150,000 VND
                CreditsCount = 15,
                PriorityLevel = 2,
                MaxImages = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new PackageDefinition
            {
                Name = "Luxury",
                Price = 300000, // 300,000 VND
                CreditsCount = 10,
                PriorityLevel = 3,
                MaxImages = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.PackageDefinitions.AddRangeAsync(packages);
        await _context.SaveChangesAsync(); // Save để có ID
        _logger.LogInformation("Đã seed {Count} package definitions", packages.Count);
    }

    /// <summary>
    /// Seed Contract Templates
    /// </summary>
    private async Task SeedContractTemplatesAsync()
    {
        if (await _context.ContractTemplates.AnyAsync())
        {
            _logger.LogInformation("Contract Templates đã tồn tại, bỏ qua seed.");
            return;
        }

        var electricVehicleCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Code == "ELECTRIC_VEHICLE");
        var batteryCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Code == "BATTERY");

        if (electricVehicleCategory == null || batteryCategory == null)
        {
            _logger.LogWarning("Categories chưa được seed, không thể seed Contract Templates.");
            return;
        }

        var templates = new List<ContractTemplate>
        {
            new ContractTemplate
            {
                TemplateName = "Hợp đồng mua bán xe điện",
                TemplateContent = GetElectricVehicleContractTemplate(),
                CategoryId = electricVehicleCategory.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ContractTemplate
            {
                TemplateName = "Hợp đồng mua bán pin",
                TemplateContent = GetBatteryContractTemplate(),
                CategoryId = batteryCategory.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.ContractTemplates.AddRangeAsync(templates);
        _logger.LogInformation("Đã seed {Count} contract templates", templates.Count);
    }

    /// <summary>
    /// Seed Admin User mặc định
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        var adminEmail = "admin@evehicle.vn";
        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            _logger.LogInformation("Admin user đã tồn tại, bỏ qua seed.");
            return;
        }

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PhoneNumber = "0900000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Mật khẩu mặc định: Admin@123
            FullName = "Administrator",
            Role = "ADMIN",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(adminUser);
        _logger.LogInformation("Đã seed Admin user: {Email}", adminEmail);
    }

    /// <summary>
    /// Seed Fake Users để tạo dữ liệu test
    /// </summary>
    private async Task SeedFakeUsersAsync()
    {
        // Kiểm tra xem đã có users nào chưa (trừ admin)
        var existingUserCount = await _context.Users.CountAsync(u => u.Role != "ADMIN");
        
        if (existingUserCount >= 10)
        {
            _logger.LogInformation("Đã có đủ fake users ({Count}), bỏ qua seed.", existingUserCount);
            return;
        }

        var fakeUsers = new List<User>();
        var random = new Random();
        
        var firstNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng" };
        var lastNames = new[] { "Văn An", "Thị Bình", "Văn Cường", "Thị Dung", "Văn Đức", "Thị Hoa", "Văn Hùng", "Thị Lan", "Văn Minh", "Thị Nga" };
        var locations = new[] { "Hà Nội", "Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Huế", "Nha Trang", "Vũng Tàu" };

        for (int i = 0; i < 10; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var fullName = $"{firstName} {lastName}";
            var email = $"user{i + 1}@evehicle.vn";
            var phoneNumber = $"09{random.Next(10000000, 99999999)}";

            // Kiểm tra xem user đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == email || u.PhoneNumber == phoneNumber))
            {
                continue;
            }

            fakeUsers.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), // Mật khẩu mặc định: User@123
                FullName = fullName,
                Address = $"{locations[random.Next(locations.Length)]}",
                Role = "MEMBER",
                Status = "ACTIVE",
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
            });
        }

        if (fakeUsers.Any())
        {
            await _context.Users.AddRangeAsync(fakeUsers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã seed {Count} fake users", fakeUsers.Count);
        }
    }

    /// <summary>
    /// Seed Fake Posts
    /// </summary>
    private async Task SeedPostsAsync()
    {
        // Kiểm tra xem đã có posts chưa
        if (await _context.Posts.AnyAsync())
        {
            _logger.LogInformation("Posts đã tồn tại, bỏ qua seed.");
            return;
        }

        // Lấy categories
        var electricVehicleCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Code == "ELECTRIC_VEHICLE");
        var batteryCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Code == "BATTERY");

        if (electricVehicleCategory == null || batteryCategory == null)
        {
            _logger.LogWarning("Categories chưa được seed, không thể seed Posts.");
            return;
        }

        // Lấy users (trừ admin)
        var users = await _context.Users
            .Where(u => u.Role == "MEMBER" && u.Status == "ACTIVE")
            .ToListAsync();

        if (!users.Any())
        {
            _logger.LogWarning("Chưa có users, không thể seed Posts.");
            return;
        }

        // Lấy admin user để approve một số posts
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "ADMIN");

        // Lấy packages
        var packages = await _context.PackageDefinitions.ToListAsync();
        var basicPackage = packages.FirstOrDefault(p => p.Name == "Basic");
        var premiumPackage = packages.FirstOrDefault(p => p.Name == "Premium");
        var luxuryPackage = packages.FirstOrDefault(p => p.Name == "Luxury");

        var random = new Random();
        var posts = new List<Post>();

        // Dữ liệu mẫu cho Xe điện
        var electricVehicleBrands = new[] { "VinFast", "Pega", "Yamaha", "Honda", "SYM", "Piaggio", "Kawasaki", "Ducati" };
        var electricVehicleModels = new[] { "Klara", "Ludo", "Impes", "Feliz", "Theon", "Vespa", "NVX", "Lead" };
        var locations = new[] { "Hà Nội", "Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Huế", "Nha Trang", "Vũng Tàu", "Biên Hòa", "Hải Dương" };
        var conditions = new[] { "Mới", "Như mới", "Tốt", "Khá", "Trung bình" };
        var statuses = new[] { "PENDING", "APPROVED", "DENIED" };

        // Tạo 20 posts cho Xe điện
        for (int i = 0; i < 20; i++)
        {
            var user = users[random.Next(users.Count)];
            var brand = electricVehicleBrands[random.Next(electricVehicleBrands.Length)];
            var model = electricVehicleModels[random.Next(electricVehicleModels.Length)];
            var year = random.Next(2020, 2025);
            var mileage = random.Next(0, 50000);
            var batteryCapacity = (decimal)(random.Next(30, 100) * 0.1); // 3.0 - 10.0 kWh
            var chargeCount = random.Next(0, 500);
            var condition = conditions[random.Next(conditions.Length)];
            var location = locations[random.Next(locations.Length)];
            var price = random.Next(10000000, 50000000); // 10M - 50M VND
            var status = statuses[random.Next(statuses.Length)];
            var selectedPackage = new[] { basicPackage, premiumPackage, luxuryPackage }[random.Next(3)];

            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CategoryId = electricVehicleCategory.CategoryId,
                Title = $"{brand} {model} {year} - {condition}",
                Description = $"Xe điện {brand} {model} năm {year}, tình trạng {condition.ToLower()}. Dung lượng pin hiện tại: {batteryCapacity:F1} kWh, đã sạc {chargeCount} lần. Số km đã đi: {mileage} km. Xe được bảo quản tốt, còn mới. Liên hệ để xem xe và thương lượng.",
                Price = price,
                SuggestedPrice = price + random.Next(-1000000, 2000000),
                Location = location,
                Brand = brand,
                Model = model,
                BatteryCapacityCurrent = batteryCapacity,
                ChargeCount = chargeCount,
                ProductionYear = year,
                Condition = condition,
                Mileage = mileage,
                AuctionEnabled = random.Next(0, 4) == 0, // 25% có auction
                StartingBid = random.Next(0, 4) == 0 ? price * 0.8m : null,
                BuyNowPrice = random.Next(0, 4) == 0 ? price * 1.1m : null,
                AuctionEndTime = random.Next(0, 4) == 0 ? DateTime.UtcNow.AddDays(random.Next(1, 7)) : null,
                Status = status,
                IsActive = status == "APPROVED",
                IsSold = false,
                SelectedPackageId = selectedPackage?.PackageId,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                BumpedAt = DateTime.UtcNow.AddDays(-random.Next(0, 7))
            };

            // Nếu approved, set thông tin approval
            if (status == "APPROVED" && adminUser != null)
            {
                post.ApprovedAt = post.CreatedAt.AddHours(random.Next(1, 24));
                post.ApprovedBy = adminUser.Id;
            }

            // Nếu denied, set thông tin rejection
            if (status == "DENIED" && adminUser != null)
            {
                post.RejectedAt = post.CreatedAt.AddHours(random.Next(1, 24));
                post.RejectedBy = adminUser.Id;
                post.RejectionReason = "Thông tin không đầy đủ hoặc không phù hợp với chính sách của chúng tôi.";
            }

            posts.Add(post);
        }

        // Dữ liệu mẫu cho Pin
        var batteryBrands = new[] { "VinFast", "Pega", "Yamaha", "Honda", "SYM", "LG", "Samsung", "Panasonic" };
        var batteryModels = new[] { "Lithium 48V", "Lithium 60V", "Lithium 72V", "Lead-Acid 48V", "Lead-Acid 60V", "Gel Battery", "AGM Battery" };

        // Tạo 15 posts cho Pin
        for (int i = 0; i < 15; i++)
        {
            var user = users[random.Next(users.Count)];
            var brand = batteryBrands[random.Next(batteryBrands.Length)];
            var model = batteryModels[random.Next(batteryModels.Length)];
            var year = random.Next(2021, 2025);
            var batteryCapacity = (decimal)(random.Next(20, 80) * 0.1); // 2.0 - 8.0 kWh
            var chargeCount = random.Next(50, 800);
            var condition = conditions[random.Next(conditions.Length)];
            var location = locations[random.Next(locations.Length)];
            var price = random.Next(2000000, 15000000); // 2M - 15M VND
            var status = statuses[random.Next(statuses.Length)];
            var selectedPackage = new[] { basicPackage, premiumPackage, luxuryPackage }[random.Next(3)];

            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CategoryId = batteryCategory.CategoryId,
                Title = $"{brand} {model} - Pin {condition}",
                Description = $"Pin {brand} {model} năm {year}, tình trạng {condition.ToLower()}. Dung lượng pin: {batteryCapacity:F1} kWh, đã sạc {chargeCount} lần. Pin còn tốt, sử dụng ổn định. Liên hệ để mua.",
                Price = price,
                SuggestedPrice = price + random.Next(-500000, 1000000),
                Location = location,
                Brand = brand,
                Model = model,
                BatteryCapacityCurrent = batteryCapacity,
                ChargeCount = chargeCount,
                ProductionYear = year,
                Condition = condition,
                Mileage = null, // Pin không có mileage
                AuctionEnabled = random.Next(0, 5) == 0, // 20% có auction
                StartingBid = random.Next(0, 5) == 0 ? price * 0.8m : null,
                BuyNowPrice = random.Next(0, 5) == 0 ? price * 1.1m : null,
                AuctionEndTime = random.Next(0, 5) == 0 ? DateTime.UtcNow.AddDays(random.Next(1, 7)) : null,
                Status = status,
                IsActive = status == "APPROVED",
                IsSold = false,
                SelectedPackageId = selectedPackage?.PackageId,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                BumpedAt = DateTime.UtcNow.AddDays(-random.Next(0, 7))
            };

            // Nếu approved, set thông tin approval
            if (status == "APPROVED" && adminUser != null)
            {
                post.ApprovedAt = post.CreatedAt.AddHours(random.Next(1, 24));
                post.ApprovedBy = adminUser.Id;
            }

            // Nếu denied, set thông tin rejection
            if (status == "DENIED" && adminUser != null)
            {
                post.RejectedAt = post.CreatedAt.AddHours(random.Next(1, 24));
                post.RejectedBy = adminUser.Id;
                post.RejectionReason = "Thông tin không đầy đủ hoặc không phù hợp với chính sách của chúng tôi.";
            }

            posts.Add(post);
        }

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Đã seed {Count} fake posts ({ElectricVehicleCount} xe điện, {BatteryCount} pin)", 
            posts.Count, 
            posts.Count(p => p.CategoryId == electricVehicleCategory.CategoryId),
            posts.Count(p => p.CategoryId == batteryCategory.CategoryId));
    }

    /// <summary>
    /// Lấy nội dung mẫu hợp đồng mua bán xe điện
    /// </summary>
    private static string GetElectricVehicleContractTemplate()
    {
        return @"HỢP ĐỒNG MUA BÁN XE ĐIỆN

Số hợp đồng: {CONTRACT_NUMBER}
Ngày ký: {SIGN_DATE}

Bên bán (Seller):
- Họ tên: {SELLER_NAME}
- CMND/CCCD: {SELLER_ID_NUMBER}
- Địa chỉ: {SELLER_ADDRESS}
- Số điện thoại: {SELLER_PHONE}

Bên mua (Buyer):
- Họ tên: {BUYER_NAME}
- CMND/CCCD: {BUYER_ID_NUMBER}
- Địa chỉ: {BUYER_ADDRESS}
- Số điện thoại: {BUYER_PHONE}

Thông tin sản phẩm:
- Loại sản phẩm: Xe điện
- Thương hiệu: {BRAND}
- Model: {MODEL}
- Năm sản xuất: {PRODUCTION_YEAR}
- Dung lượng pin hiện tại: {BATTERY_CAPACITY} kWh
- Số lần sạc: {CHARGE_COUNT}
- Số KM đã đi: {MILEAGE} km
- Tình trạng: {CONDITION}

Giá mua bán: {PRICE} VND

Điều khoản:
1. Bên bán cam kết thông tin sản phẩm đúng như mô tả.
2. Bên mua đã kiểm tra và đồng ý mua sản phẩm.
3. Giao dịch được thực hiện thông qua nền tảng EVehicle.
4. Hai bên đồng ý với các điều khoản trên.

Chữ ký bên bán: {SELLER_SIGNATURE}
Chữ ký bên mua: {BUYER_SIGNATURE}
";
    }

    /// <summary>
    /// Lấy nội dung mẫu hợp đồng mua bán pin
    /// </summary>
    private static string GetBatteryContractTemplate()
    {
        return @"HỢP ĐỒNG MUA BÁN PIN

Số hợp đồng: {CONTRACT_NUMBER}
Ngày ký: {SIGN_DATE}

Bên bán (Seller):
- Họ tên: {SELLER_NAME}
- CMND/CCCD: {SELLER_ID_NUMBER}
- Địa chỉ: {SELLER_ADDRESS}
- Số điện thoại: {SELLER_PHONE}

Bên mua (Buyer):
- Họ tên: {BUYER_NAME}
- CMND/CCCD: {BUYER_ID_NUMBER}
- Địa chỉ: {BUYER_ADDRESS}
- Số điện thoại: {BUYER_PHONE}

Thông tin sản phẩm:
- Loại sản phẩm: Pin
- Thương hiệu: {BRAND}
- Model: {MODEL}
- Năm sản xuất: {PRODUCTION_YEAR}
- Dung lượng pin hiện tại: {BATTERY_CAPACITY} kWh
- Số lần sạc: {CHARGE_COUNT}
- Tình trạng: {CONDITION}

Giá mua bán: {PRICE} VND

Điều khoản:
1. Bên bán cam kết thông tin sản phẩm đúng như mô tả.
2. Bên mua đã kiểm tra và đồng ý mua sản phẩm.
3. Giao dịch được thực hiện thông qua nền tảng EVehicle.
4. Hai bên đồng ý với các điều khoản trên.

Chữ ký bên bán: {SELLER_SIGNATURE}
Chữ ký bên mua: {BUYER_SIGNATURE}
";
    }
}

