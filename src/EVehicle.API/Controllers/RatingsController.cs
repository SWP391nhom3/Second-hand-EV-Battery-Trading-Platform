using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Ratings;
using EVehicle.Application.Interfaces;
using EVehicle.API.Helpers;
using EVehicle.API.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý đánh giá và phản hồi
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Ratings")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly IValidator<RatingCreateRequest> _createValidator;
    private readonly IValidator<RatingUpdateRequest> _updateValidator;
    private readonly IValidator<RatingReplyRequest> _replyValidator;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(
        IRatingService ratingService,
        IValidator<RatingCreateRequest> createValidator,
        IValidator<RatingUpdateRequest> updateValidator,
        IValidator<RatingReplyRequest> replyValidator,
        ILogger<RatingsController> logger)
    {
        _ratingService = ratingService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _replyValidator = replyValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC31: Người mua đánh giá người bán
    /// </summary>
    [HttpPost("rate-seller")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<RatingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RateSeller([FromBody] RatingCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<RatingResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<RatingResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _ratingService.RateSellerAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetRatingById),
                    new { id = response.Data?.RatingId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá người bán");
            return StatusCode(500, BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh giá người bán"));
        }
    }

    /// <summary>
    /// UC32: Người bán đánh giá người mua
    /// </summary>
    [HttpPost("rate-buyer")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<RatingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RateBuyer([FromBody] RatingCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<RatingResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<RatingResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _ratingService.RateBuyerAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetRatingById),
                    new { id = response.Data?.RatingId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá người mua");
            return StatusCode(500, BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh giá người mua"));
        }
    }

    /// <summary>
    /// UC33: Chỉnh sửa đánh giá (trong vòng 7 ngày)
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<RatingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRating(
        Guid id,
        [FromBody] RatingUpdateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<RatingResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<RatingResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _ratingService.UpdateRatingAsync(userId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("Không tìm thấy"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chỉnh sửa đánh giá, RatingId: {RatingId}", id);
            return StatusCode(500, BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi chỉnh sửa đánh giá"));
        }
    }

    /// <summary>
    /// UC34: Phản hồi đánh giá
    /// </summary>
    [HttpPost("{id}/reply")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<RatingReplyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReplyToRating(
        Guid id,
        [FromBody] RatingReplyRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _replyValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _ratingService.ReplyToRatingAsync(userId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetRatingById),
                    new { id = id },
                    response);
            }

            if (response.Message.Contains("Không tìm thấy"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phản hồi đánh giá, RatingId: {RatingId}", id);
            return StatusCode(500, BaseResponse<RatingReplyResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi phản hồi đánh giá"));
        }
    }

    /// <summary>
    /// Lấy chi tiết đánh giá
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<RatingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRatingById(Guid id)
    {
        try
        {
            var response = await _ratingService.GetRatingByIdAsync(id);
            
            if (response.Success)
            {
                return Ok(response);
            }

            return NotFound(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết đánh giá, RatingId: {RatingId}", id);
            return StatusCode(500, BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết đánh giá"));
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá (có phân trang)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<RatingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRatings([FromQuery] RatingSearchRequest request)
    {
        try
        {
            var response = await _ratingService.GetRatingsAsync(request);
            
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đánh giá");
            return StatusCode(500, PagedResponse<RatingResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách đánh giá"));
        }
    }
}


