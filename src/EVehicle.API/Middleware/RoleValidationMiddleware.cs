using System.Security.Claims;
using EVehicle.API.Helpers;

namespace EVehicle.API.Middleware;

/// <summary>
/// Middleware để log và validate role từ JWT token
/// Lưu ý: ASP.NET Core Authorization middleware sẽ xử lý authorization thực sự,
/// middleware này chỉ để log và thêm validation logic nếu cần
/// </summary>
public class RoleValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleValidationMiddleware> _logger;
    private readonly HashSet<string> _allowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "MEMBER",
        "STAFF",
        "ADMIN"
    };

    public RoleValidationMiddleware(
        RequestDelegate next,
        ILogger<RoleValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging cho các endpoint không cần authentication
        if (IsAnonymousEndpoint(context))
        {
            await _next(context);
            return;
        }

        // Chỉ log thông tin, không block request (Authorization middleware sẽ xử lý)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userInfo = ClaimsHelper.GetUserInfo(context.User);
            var endpoint = context.GetEndpoint();
            var authorizeAttributes = endpoint?
                .Metadata
                .GetOrderedMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            if (userInfo != null)
            {
                var requiredRoles = authorizeAttributes?
                    .Where(a => !string.IsNullOrEmpty(a.Roles))
                    .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(r => r.Trim())
                    .Distinct()
                    .ToList() ?? new List<string>();

                if (requiredRoles.Any())
                {
                    _logger.LogInformation(
                        "User {UserId} với role {UserRole} đang truy cập endpoint yêu cầu roles {RequiredRoles}. Path: {Path}",
                        userInfo.UserId,
                        userInfo.Role,
                        string.Join(", ", requiredRoles),
                        context.Request.Path);
                }
                else
                {
                    _logger.LogDebug(
                        "User {UserId} với role {UserRole} đang truy cập endpoint. Path: {Path}",
                        userInfo.UserId,
                        userInfo.Role,
                        context.Request.Path);
                }

                // Validate role có hợp lệ không (chỉ log warning, không block)
                if (!string.IsNullOrEmpty(userInfo.Role) && !_allowedRoles.Contains(userInfo.Role))
                {
                    _logger.LogWarning(
                        "User {UserId} có role không hợp lệ: {Role}. Path: {Path}",
                        userInfo.UserId,
                        userInfo.Role,
                        context.Request.Path);
                }
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Kiểm tra endpoint có được đánh dấu là anonymous không
    /// </summary>
    private static bool IsAnonymousEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
            return false;

        var allowAnonymous = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
        return allowAnonymous != null;
    }
}

/// <summary>
/// Extension method để đăng ký RoleValidationMiddleware
/// </summary>
public static class RoleValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRoleValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RoleValidationMiddleware>();
    }
}

