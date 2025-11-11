using EVehicle.Application.Interfaces;
using EVehicle.Application.Services;
using EVehicle.Application.Validators.Auth;
using EVehicle.Application.Validators.Posts;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EVehicle.Application;

/// <summary>
/// Extension methods để đăng ký Application Layer services
/// </summary>
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();

        // Đăng ký FluentValidation Validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<VerifyEmailRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ResendOtpRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PostCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PostUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PostCompareRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PostApproveRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PostRejectRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Bids.BidCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Leads.LeadCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Leads.LeadStatusUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Leads.LeadAssignStaffRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Orders.OrderCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Orders.PaymentCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Contracts.ContractCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Contracts.ContractSignRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Appointments.AppointmentCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Appointments.AppointmentUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Appointments.AppointmentStatusUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Ratings.RatingCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Ratings.RatingUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Ratings.RatingReplyRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Chat.MessageCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Chat.ChatHistoryRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Notifications.NotificationMarkReadRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Users.UserSearchRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Users.UserUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Users.UpdateProfileRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Packages.PackageCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Packages.PackageUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EVehicle.Application.Validators.Posts.PriceSuggestionRequestValidator>();

        return services;
    }
}

