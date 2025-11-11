using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Users;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý người dùng (UC47)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Users")]
[Authorize] // Yêu cầu authentication cho tất cả endpoints
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserSearchRequest> _searchValidator;
    private readonly IValidator<UserUpdateRequest> _updateValidator;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IValidator<UserSearchRequest> searchValidator,
        IValidator<UserUpdateRequest> updateValidator,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _searchValidator = searchValidator;
        _updateValidator = updateValidator;
        _updateProfileValidator = updateProfileValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC47.1: Lấy danh sách người dùng với phân trang và lọc (Chỉ ADMIN)
    /// </summary>
    [HttpGet]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromQuery] UserSearchRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _searchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(PagedResponse<UserResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            var response = await _userService.GetUsersAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
            return StatusCode(500, PagedResponse<UserResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách người dùng"));
        }
    }

    /// <summary>
    /// UC47.4: Lấy chi tiết thông tin người dùng
    /// MEMBER: Chỉ xem được thông tin của chính mình
    /// STAFF/ADMIN: Xem được thông tin của tất cả người dùng
    /// </summary>
    [HttpGet("{id}")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var currentUserInfo = ClaimsHelper.GetUserInfo(User);
            if (currentUserInfo == null)
            {
                return Unauthorized(BaseResponse<UserResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // MEMBER chỉ được xem thông tin của chính mình
            if (currentUserInfo.Role == "MEMBER" && currentUserInfo.UserId != id)
            {
                return Forbid(); // Trả về 403 Forbidden
            }

            var response = await _userService.GetUserByIdAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng {UserId}", id);
            return StatusCode(500, BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// Lấy thông tin người dùng hiện tại (từ JWT token)
    /// </summary>
    [HttpGet("me")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userInfo = ClaimsHelper.GetUserInfo(User);
            if (userInfo == null)
            {
                return Unauthorized(BaseResponse<UserResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            var response = await _userService.GetUserByIdAsync(userInfo.UserId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng hiện tại");
            return StatusCode(500, BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// UC47.2 &amp; UC47.3: Cập nhật thông tin người dùng (vô hiệu hóa/kích hoạt, thay đổi role) - Chỉ ADMIN
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<UserResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            var adminInfo = ClaimsHelper.GetUserInfo(User);
            _logger.LogWarning(
                "Admin {AdminId} đang cập nhật thông tin người dùng {UserId}",
                adminInfo?.UserId,
                id);

            var response = await _userService.UpdateUserAsync(id, request);

            if (!response.Success)
            {
                if (response.Message.Contains("Không tìm thấy"))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            _logger.LogInformation(
                "Admin {AdminId} đã cập nhật thông tin người dùng {UserId} thành công",
                adminInfo?.UserId,
                id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng {UserId}", id);
            return StatusCode(500, BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật thông tin người dùng"));
        }
    }

    /// <summary>
    /// UC47.5: Lấy lịch sử hoạt động của người dùng - Chỉ ADMIN
    /// </summary>
    [HttpGet("{id}/activity")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<List<UserActivityResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserActivity(Guid id)
    {
        try
        {
            var response = await _userService.GetUserActivityAsync(id);

            if (!response.Success)
            {
                if (response.Message.Contains("Không tìm thấy"))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử hoạt động của người dùng {UserId}", id);
            return StatusCode(500, BaseResponse<List<UserActivityResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy lịch sử hoạt động"));
        }
    }

    /// <summary>
    /// UC04: Member tự cập nhật thông tin profile của mình
    /// </summary>
    [HttpPut("me")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userInfo = ClaimsHelper.GetUserInfo(User);
            if (userInfo == null)
            {
                return Unauthorized(BaseResponse<UserResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // Validate request
            var validationResult = await _updateProfileValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<UserResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            var response = await _userService.UpdateMyProfileAsync(userInfo.UserId, request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật profile");
            return StatusCode(500, BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật thông tin profile"));
        }
    }

    /// <summary>
    /// UC04: Upload avatar cho user hiện tại
    /// </summary>
    [HttpPost("me/avatar")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
    {
        try
        {
            var userInfo = ClaimsHelper.GetUserInfo(User);
            if (userInfo == null)
            {
                return Unauthorized(BaseResponse<string>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(BaseResponse<string>.FailureResponse(
                    "File không được để trống"));
            }

            // Convert IFormFile to FileUploadDto
            var fileDto = await FileHelper.ConvertToFileUploadDtoAsync(file);
            if (fileDto == null)
            {
                return BadRequest(BaseResponse<string>.FailureResponse(
                    "Không thể xử lý file"));
            }

            var response = await _userService.UploadAvatarAsync(userInfo.UserId, fileDto);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload avatar");
            return StatusCode(500, BaseResponse<string>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi upload avatar"));
        }
    }
}

