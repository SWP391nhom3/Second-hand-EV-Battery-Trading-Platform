using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Favorites;
using EVehicle.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý yêu thích
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Favorites")]
[AuthorizeRoles("MEMBER")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoriteService favoriteService,
        ILogger<FavoritesController> logger)
    {
        _favoriteService = favoriteService;
        _logger = logger;
    }

    /// <summary>
    /// UC18: Thêm bài đăng vào danh sách yêu thích
    /// </summary>
    /// <param name="postId">ID bài đăng</param>
    /// <returns>Thông tin yêu thích đã tạo</returns>
    [HttpPost("{postId}")]
    [ProducesResponseType(typeof(BaseResponse<FavoriteResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToFavorites([FromRoute] Guid postId)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<FavoriteResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _favoriteService.AddToFavoritesAsync(userId.Value, postId);

            // 3. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetFavorites),
                    new { },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm vào yêu thích, PostId: {PostId}", postId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                BaseResponse<FavoriteResponse>.FailureResponse(
                    ex,
                    "Đã xảy ra lỗi khi thêm vào yêu thích"));
        }
    }

    /// <summary>
    /// UC19: Xóa bài đăng khỏi danh sách yêu thích
    /// </summary>
    /// <param name="postId">ID bài đăng</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{postId}")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromFavorites([FromRoute] Guid postId)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _favoriteService.RemoveFromFavoritesAsync(userId.Value, postId);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("không có trong danh sách"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa khỏi yêu thích, PostId: {PostId}", postId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                BaseResponse.FailureResponse(
                    ex,
                    "Đã xảy ra lỗi khi xóa khỏi yêu thích"));
        }
    }

    /// <summary>
    /// UC22: Xem danh sách yêu thích
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách yêu thích</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<FavoriteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavorites([FromQuery] FavoriteListRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<FavoriteResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            request.IsValid();

            // 3. Call service
            var response = await _favoriteService.GetFavoritesAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách yêu thích");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                PagedResponse<FavoriteResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi lấy danh sách yêu thích"));
        }
    }

    /// <summary>
    /// Kiểm tra xem bài đăng đã được thêm vào yêu thích chưa
    /// </summary>
    /// <param name="postId">ID bài đăng</param>
    /// <returns>True nếu đã yêu thích, False nếu chưa</returns>
    [HttpGet("{postId}/check")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckFavorite([FromRoute] Guid postId)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<bool>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _favoriteService.IsFavoriteAsync(userId.Value, postId);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra yêu thích, PostId: {PostId}", postId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                BaseResponse<bool>.FailureResponse(
                    ex,
                    "Đã xảy ra lỗi khi kiểm tra yêu thích"));
        }
    }
}

