using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using EVehicle.Infrastructure.Data;
using EVehicle.Application.Interfaces;
using EVehicle.Infrastructure.Repositories;
using EVehicle.Infrastructure.Services;

namespace EVehicle.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<EVehicleDbContext>(options =>
            options.UseSqlServer(connectionString, b => 
                b.MigrationsAssembly("EVehicle.Infrastructure")));

        // Repository Registration
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailVerificationOtpRepository, EmailVerificationOtpRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IUserPackageCreditsRepository, UserPackageCreditsRepository>();
        services.AddScoped<IPostStaffAssignmentRepository, PostStaffAssignmentRepository>();
        services.AddScoped<IPostSubscriptionRepository, PostSubscriptionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IContractTemplateRepository, ContractTemplateRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IRatingReplyRepository, RatingReplyRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IAIPriceSuggestionRepository, AIPriceSuggestionRepository>();

        // Infrastructure Services Registration
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        
        // AI Price Service với HttpClient
        services.AddHttpClient<IAIPriceService, AIPriceService>();
        
        // PayOS Service với HttpClient
        services.AddHttpClient<Application.Interfaces.IPayOSService, PayOSService>();
        
        // File Storage Service - Cần cấu hình root path từ API layer
        // Root path sẽ được cấu hình trong API layer (có thể truy cập IWebHostEnvironment)
        
        // HttpClient for SocialAuthService
        // AddHttpClient sẽ đăng ký HttpClient và SocialAuthService
        services.AddHttpClient<ISocialAuthService, SocialAuthService>();

        // DbSeeder Registration
        services.AddScoped<Data.DbSeeder>();

        return services;
    }

    /// <summary>
    /// Seed dữ liệu ban đầu cho database
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.EVehicleDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Cho phép drop và recreate DB trong môi trường dev nếu được bật trong cấu hình
        var shouldDropAndRecreate = configuration.GetSection("Database").GetValue<bool>("DropAndRecreate", false);
        if (shouldDropAndRecreate)
        {
            // Chỉ nên dùng trong môi trường phát triển
            await dbContext.Database.EnsureDeletedAsync();
        }
        
        // Đảm bảo schema luôn được tạo/được nâng cấp trước khi seed
        await dbContext.Database.MigrateAsync();
        
        var seeder = scope.ServiceProvider.GetRequiredService<Data.DbSeeder>();
        await seeder.SeedAsync();
    }
}

