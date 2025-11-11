using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Users;

/// <summary>
/// Request DTO cho tìm kiếm người dùng (UC47)
/// </summary>
public class UserSearchRequest : PagedRequest
{
    /// <summary>
    /// Từ khóa tìm kiếm (email, phone, fullName)
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Lọc theo role (MEMBER, STAFF, ADMIN)
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Lọc theo status (ACTIVE, BANNED, SUSPENDED, PENDING_VERIFICATION)
    /// </summary>
    public string? Status { get; set; }
}

