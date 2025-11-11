using System.Security.Claims;

namespace EVehicle.API.Middleware;

/// <summary>
/// Middleware để debug authentication và log thông tin chi tiết
/// </summary>
public class AuthenticationDebugMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationDebugMiddleware> _logger;
    private readonly HashSet<string> _debugPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/packages/my-packages",
        "/api/posts"
    };

    public AuthenticationDebugMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationDebugMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
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
            await _next(context);
            return;
        }

        // Chỉ log cho các paths cần debug
        if (_debugPaths.Contains(path))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
            var claimsCount = context.User?.Claims?.Count() ?? 0;
            var userId = context.User?.FindFirst("userId")?.Value;
            var role = context.User?.FindFirst("role")?.Value;

            _logger.LogInformation(
                "Authentication Debug - Path: {Path}, Method: {Method}, " +
                "IsAuthenticated: {IsAuthenticated}, ClaimsCount: {ClaimsCount}, " +
                "UserId: {UserId}, Role: {Role}, " +
                "HasAuthHeader: {HasAuthHeader}, AuthHeaderPrefix: {AuthHeaderPrefix}",
                context.Request.Path,
                context.Request.Method,
                isAuthenticated,
                claimsCount,
                userId ?? "null",
                role ?? "null",
                !string.IsNullOrEmpty(authHeader),
                authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true ? "Bearer" : "None");

            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring(7).Trim();
                    _logger.LogDebug("Token preview: {TokenPreview}...", 
                        token.Length > 30 ? token.Substring(0, 30) : token);
                }
                else
                {
                    _logger.LogWarning("Authorization header không bắt đầu bằng 'Bearer '. Header: {Header}",
                        authHeader.Length > 50 ? authHeader.Substring(0, 50) : authHeader);
                }
            }
            else
            {
                _logger.LogWarning("Không có Authorization header trong request");
            }

            // Log tất cả claims nếu có
            if (context.User?.Claims != null && context.User.Claims.Any())
            {
                var allClaims = context.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                _logger.LogDebug("All claims: {Claims}", string.Join(", ", allClaims));
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method để đăng ký AuthenticationDebugMiddleware
/// </summary>
public static class AuthenticationDebugMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationDebug(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationDebugMiddleware>();
    }
}

