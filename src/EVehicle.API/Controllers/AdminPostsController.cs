using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý bài đăng dành cho Admin
/// </summary>
[ApiController]
[Route("api/admin/posts")]
[Produces("application/json")]
[Tags("Admin - Posts")]
[AuthorizeRoles("ADMIN")]
public class AdminPostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IValidator<PostApproveRequest> _approveValidator;
    private readonly IValidator<PostRejectRequest> _rejectValidator;
    private readonly ILogger<AdminPostsController> _logger;

    public AdminPostsController(
        IPostService postService,
        IValidator<PostApproveRequest> approveValidator,
        IValidator<PostRejectRequest> rejectValidator,
        ILogger<AdminPostsController> logger)
    {
        _postService = postService;
        _approveValidator = approveValidator;
        _rejectValidator = rejectValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC11: Duyệt bài đăng
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <param name="request">Thông tin duyệt bài đăng (không cần thông tin gì, chỉ cần duyệt)</param>
    /// <returns>Thông tin bài đăng đã được duyệt</returns>
    /// <remarks>
    /// Staff sẽ được tự động gán khi có người yêu cầu tư vấn (tạo Lead)
    /// </remarks>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(BaseResponse<PostDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApprovePost(Guid id, [FromBody] PostApproveRequest request)
    {
        try
        {
            // 1. Get adminId from JWT token
            var adminId = ClaimsHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(BaseResponse<PostDetailResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _approveValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PostDetailResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _postService.ApprovePostAsync(adminId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi duyệt bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse<PostDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi duyệt bài đăng"));
        }
    }

    /// <summary>
    /// UC12: Từ chối bài đăng
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <param name="request">Thông tin từ chối (Lý do từ chối)</param>
    /// <returns>Kết quả từ chối bài đăng</returns>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RejectPost(Guid id, [FromBody] PostRejectRequest request)
    {
        try
        {
            // 1. Get adminId from JWT token
            var adminId = ClaimsHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(BaseResponse.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _rejectValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _postService.RejectPostAsync(adminId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi từ chối bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi từ chối bài đăng"));
        }
    }

    /// <summary>
    /// Lấy danh sách bài đăng chờ duyệt (có phân trang)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách bài đăng chờ duyệt</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResponse<PendingPostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPendingPosts([FromQuery] PendingPostSearchRequest request)
    {
        try
        {
            // 1. Get adminId from JWT token
            var adminId = ClaimsHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(PagedResponse<PendingPostResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _postService.GetPendingPostsAsync(adminId.Value, request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng chờ duyệt");
            return StatusCode(500, PagedResponse<PendingPostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng chờ duyệt"));
        }
    }

    /// <summary>
    /// Lấy chi tiết bài đăng (bao gồm thông tin Staff và Subscription)
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <returns>Chi tiết bài đăng</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<PostDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        try
        {
            var response = await _postService.GetPostByIdAsync(id);

            if (response.Success)
            {
                return Ok(response);
            }

            return NotFound(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse<PostDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết bài đăng"));
        }
    }

    /// <summary>
    /// Lấy danh sách bài đăng đã duyệt hoặc từ chối (có phân trang)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách bài đăng đã duyệt hoặc từ chối</returns>
    [HttpGet("approved-rejected")]
    [ProducesResponseType(typeof(PagedResponse<ApprovedRejectedPostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetApprovedRejectedPosts([FromQuery] ApprovedRejectedPostSearchRequest request)
    {
        try
        {
            // 1. Get adminId from JWT token
            var adminId = ClaimsHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(PagedResponse<ApprovedRejectedPostResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _postService.GetApprovedRejectedPostsAsync(adminId.Value, request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng đã duyệt/từ chối");
            return StatusCode(500, PagedResponse<ApprovedRejectedPostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng đã duyệt/từ chối"));
        }
    }
}

