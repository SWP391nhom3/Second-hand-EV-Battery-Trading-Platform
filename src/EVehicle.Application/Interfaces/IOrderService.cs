using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Order operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// UC28: Staff tạo đơn hàng
    /// </summary>
    Task<BaseResponse<OrderResponse>> CreateOrderAsync(
        Guid staffId,
        OrderCreateRequest request);

    /// <summary>
    /// Lấy chi tiết Order
    /// </summary>
    Task<BaseResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId, Guid? userId = null);

    /// <summary>
    /// UC28: Tạo thanh toán cho đơn hàng
    /// </summary>
    Task<BaseResponse<PaymentResponse>> CreatePaymentAsync(
        Guid userId,
        PaymentCreateRequest request);

    /// <summary>
    /// UC28: Webhook xử lý thanh toán
    /// </summary>
    Task<BaseResponse<PaymentResponse>> ProcessPaymentWebhookAsync(
        PaymentWebhookRequest request);

    /// <summary>
    /// UC05: Lấy danh sách Orders của Member (với filter và phân trang)
    /// </summary>
    Task<BaseResponse<PagedResponse<OrderResponse>>> GetMyOrdersAsync(
        Guid userId,
        OrderSearchRequest request);
}

