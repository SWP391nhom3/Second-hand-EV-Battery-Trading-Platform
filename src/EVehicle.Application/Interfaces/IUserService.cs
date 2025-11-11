using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Users;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho User Service (UC47)
/// </summary>
public interface IUserService
{
    /// <summary>
    /// UC47.1: Lấy danh sách người dùng với phân trang và lọc
    /// </summary>
    Task<PagedResponse<UserResponse>> GetUsersAsync(UserSearchRequest request);

    /// <summary>
    /// UC47.4: Lấy chi tiết thông tin người dùng
    /// </summary>
    Task<BaseResponse<UserResponse>> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// UC47.2: Vô hiệu hóa/kích hoạt tài khoản
    /// UC47.3: Thay đổi role của người dùng
    /// </summary>
    Task<BaseResponse<UserResponse>> UpdateUserAsync(Guid userId, UserUpdateRequest request);

    /// <summary>
    /// UC47.5: Lấy lịch sử hoạt động của người dùng
    /// </summary>
    Task<BaseResponse<List<UserActivityResponse>>> GetUserActivityAsync(Guid userId);

    /// <summary>
    /// UC04: Member tự cập nhật thông tin profile của mình
    /// </summary>
    Task<BaseResponse<UserResponse>> UpdateMyProfileAsync(Guid userId, UpdateProfileRequest request);

    /// <summary>
    /// UC04: Upload avatar cho user hiện tại
    /// </summary>
    Task<BaseResponse<string>> UploadAvatarAsync(Guid userId, DTOs.Common.FileUploadDto file);
}

