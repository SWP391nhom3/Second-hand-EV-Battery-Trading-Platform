using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý đơn hàng và thanh toán
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<OrderCreateRequest> _orderCreateValidator;
    private readonly IValidator<PaymentCreateRequest> _paymentCreateValidator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        IValidator<OrderCreateRequest> orderCreateValidator,
        IValidator<PaymentCreateRequest> paymentCreateValidator,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _orderCreateValidator = orderCreateValidator;
        _paymentCreateValidator = paymentCreateValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC28: Staff tạo đơn hàng
    /// </summary>
    /// <param name="request">Thông tin đơn hàng</param>
    /// <returns>Đơn hàng đã tạo</returns>
    [HttpPost]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<OrderResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _orderCreateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<OrderResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _orderService.CreateOrderAsync(staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = response.Data?.OrderId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo đơn hàng");
            return StatusCode(500, BaseResponse<OrderResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng
    /// </summary>
    /// <param name="id">ID đơn hàng</param>
    /// <returns>Chi tiết đơn hàng</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<OrderResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _orderService.GetOrderByIdAsync(id, userId);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng");
            return StatusCode(500, BaseResponse<OrderResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết đơn hàng"));
        }
    }

    /// <summary>
    /// UC28: Tạo thanh toán cho đơn hàng
    /// </summary>
    /// <param name="request">Thông tin thanh toán</param>
    /// <returns>Thông tin thanh toán và URL thanh toán</returns>
    [HttpPost("payment")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PaymentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _paymentCreateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PaymentResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _orderService.CreatePaymentAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo thanh toán");
            return StatusCode(500, BaseResponse<PaymentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo thanh toán"));
        }
    }

    /// <summary>
    /// UC28: Webhook xử lý thanh toán (từ cổng thanh toán)
    /// </summary>
    /// <param name="request">Thông tin từ cổng thanh toán</param>
    /// <returns>Kết quả xử lý</returns>
    [HttpPost("payment/webhook")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPaymentWebhook([FromBody] PaymentWebhookRequest request)
    {
        try
        {
            // TODO: Validate webhook signature from payment gateway for security

            // Call service
            var response = await _orderService.ProcessPaymentWebhookAsync(request);

            // Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý webhook thanh toán");
            return StatusCode(500, BaseResponse<PaymentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi xử lý thanh toán"));
        }
    }

    /// <summary>
    /// UC05: Lấy danh sách đơn hàng của Member (Lịch sử Giao dịch)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang:
    /// - TransactionType: BUY (mua), SELL (bán), null (tất cả)
    /// - Status: PENDING_PAYMENT, PAID, CONFIRMED, SHIPPING, DELIVERED, COMPLETED, CANCELLED
    /// - FromDate: Từ ngày
    /// - ToDate: Đến ngày
    /// - Keyword: Tìm kiếm theo tiêu đề bài đăng
    /// </param>
    /// <returns>Danh sách đơn hàng</returns>
    [HttpGet("my-orders")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<OrderResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyOrders([FromQuery] OrderSearchRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PagedResponse<OrderResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _orderService.GetMyOrdersAsync(userId.Value, request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng");
            return StatusCode(500, BaseResponse<PagedResponse<OrderResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách đơn hàng"));
        }
    }
}

