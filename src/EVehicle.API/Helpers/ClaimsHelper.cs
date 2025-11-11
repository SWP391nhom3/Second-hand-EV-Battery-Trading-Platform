using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EVehicle.API.Helpers;

/// <summary>
/// Helper class để lấy thông tin từ Claims Principal
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Lấy UserId từ Claims Principal
    /// </summary>
    public static Guid? GetUserId(ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        // Thử tìm userId từ nhiều claim types
        var userIdClaim = user.FindFirst("userId")?.Value 
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value; // JWT standard claim

        if (string.IsNullOrEmpty(userIdClaim))
        {
            // Debug: Log tất cả claims để tìm vấn đề
            var allClaims = user.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            // Note: Không có logger ở đây, nhưng có thể throw exception với thông tin chi tiết
            return null;
        }

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Lấy Role từ Claims Principal
    /// </summary>
    public static string? GetRole(ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        return user.FindFirst("role")?.Value 
            ?? user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Lấy Email từ Claims Principal
    /// </summary>
    public static string? GetEmail(ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        return user.FindFirst("email")?.Value 
            ?? user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Lấy PhoneNumber từ Claims Principal
    /// </summary>
    public static string? GetPhoneNumber(ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        return user.FindFirst("phoneNumber")?.Value 
            ?? user.FindFirst(ClaimTypes.MobilePhone)?.Value;
    }

    /// <summary>
    /// Kiểm tra user có role cụ thể không
    /// </summary>
    public static bool HasRole(ClaimsPrincipal? user, string role)
    {
        if (user == null)
            return false;

        var userRole = GetRole(user);
        return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra user có bất kỳ role nào trong danh sách không
    /// </summary>
    public static bool HasAnyRole(ClaimsPrincipal? user, params string[] roles)
    {
        if (user == null || roles == null || roles.Length == 0)
            return false;

        var userRole = GetRole(user);
        if (string.IsNullOrEmpty(userRole))
            return false;

        return roles.Any(role => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Kiểm tra user có tất cả roles trong danh sách không
    /// </summary>
    public static bool HasAllRoles(ClaimsPrincipal? user, params string[] roles)
    {
        if (user == null || roles == null || roles.Length == 0)
            return false;

        var userRole = GetRole(user);
        if (string.IsNullOrEmpty(userRole))
            return false;

        // Một user chỉ có một role, nên chỉ cần check role đó có trong danh sách không
        return roles.Any(role => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lấy tất cả thông tin user từ Claims Principal
    /// </summary>
    public static UserInfo? GetUserInfo(ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        var userId = GetUserId(user);
        if (userId == null)
            return null;

        return new UserInfo
        {
            UserId = userId.Value,
            Email = GetEmail(user),
            PhoneNumber = GetPhoneNumber(user),
            Role = GetRole(user),
            FullName = user.FindFirst("fullName")?.Value,
            Status = user.FindFirst("status")?.Value
        };
    }
}

/// <summary>
/// Thông tin user từ JWT token
/// </summary>
public class UserInfo
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public string? FullName { get; set; }
    public string? Status { get; set; }
}

