namespace EVehicle.Application.DTOs.Users;

/// <summary>
/// Request DTO cho cập nhật thông tin người dùng (UC47)
/// </summary>
public class UserUpdateRequest
{
    /// <summary>
    /// Role mới (MEMBER, STAFF, ADMIN)
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Status mới (ACTIVE, BANNED, SUSPENDED, PENDING_VERIFICATION)
    /// </summary>
    public string? Status { get; set; }
}

