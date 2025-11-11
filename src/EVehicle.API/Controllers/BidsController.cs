using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Bids;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API đấu giá
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Bids")]
[AuthorizeRoles("MEMBER")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly IValidator<BidCreateRequest> _createValidator;
    private readonly ILogger<BidsController> _logger;

    public BidsController(
        IBidService bidService,
        IValidator<BidCreateRequest> createValidator,
        ILogger<BidsController> logger)
    {
        _bidService = bidService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC21: Đặt giá đấu
    /// </summary>
    /// <param name="request">Thông tin đấu giá</param>
    /// <returns>Thông tin bid đã tạo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<BidResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBid([FromBody] BidCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<BidResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<BidResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _bidService.CreateBidAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetBidsByPostId),
                    new { postId = request.PostId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đặt giá đấu");
            return StatusCode(500, BaseResponse<BidResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đặt giá đấu"));
        }
    }

    /// <summary>
    /// UC21: Lấy danh sách đấu giá của một bài đăng
    /// </summary>
    /// <param name="postId">ID bài đăng</param>
    /// <returns>Danh sách đấu giá</returns>
    [HttpGet("post/{postId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<BidListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBidsByPostId(Guid postId)
    {
        try
        {
            var response = await _bidService.GetBidsByPostIdAsync(postId);

            if (response.Success)
            {
                return Ok(response);
            }

            return NotFound(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đấu giá, PostId: {PostId}", postId);
            return StatusCode(500, BaseResponse<BidListResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách đấu giá"));
        }
    }
}

