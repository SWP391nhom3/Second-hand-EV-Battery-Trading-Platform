using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;
using EVehicle.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý thanh toán (UC30)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// UC30: Xem lịch sử thanh toán
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và filter</param>
    /// <returns>Danh sách thanh toán với phân trang</returns>
    [HttpGet]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(PagedResponse<PaymentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentHistory([FromQuery] PaymentSearchRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<PaymentDetailResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            request.IsValid();

            // 3. Call service
            var response = await _paymentService.GetPaymentHistoryAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử thanh toán");
            return StatusCode(500, PagedResponse<PaymentDetailResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy lịch sử thanh toán"));
        }
    }

    /// <summary>
    /// UC30: Xem chi tiết thanh toán
    /// </summary>
    /// <param name="id">ID thanh toán</param>
    /// <returns>Chi tiết thanh toán</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<PaymentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PaymentDetailResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _paymentService.GetPaymentByIdAsync(userId.Value, id);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết thanh toán");
            return StatusCode(500, BaseResponse<PaymentDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết thanh toán"));
        }
    }

    /// <summary>
    /// Lấy PayOS payment link và QR code
    /// </summary>
    /// <param name="id">ID thanh toán</param>
    /// <returns>PayOS payment link và QR code</returns>
    [HttpGet("{id}/payos-link")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<Application.Interfaces.PayOSPaymentLinkResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPayOSPaymentLink(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<Application.Interfaces.PayOSPaymentLinkResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _paymentService.GetPayOSPaymentLinkAsync(userId.Value, id);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy PayOS payment link");
            return StatusCode(500, BaseResponse<Application.Interfaces.PayOSPaymentLinkResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy PayOS payment link"));
        }
    }
}


