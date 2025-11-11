namespace EVehicle.Application.DTOs.Users;

/// <summary>
/// Request DTO cho Member tự cập nhật thông tin profile (UC04)
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Họ và tên
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Số CMND/CCCD
    /// </summary>
    public string? IdNumber { get; set; }
}


