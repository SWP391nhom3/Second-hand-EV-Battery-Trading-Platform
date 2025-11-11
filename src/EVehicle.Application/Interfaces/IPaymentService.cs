using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Payment Service
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// UC30: Lấy lịch sử thanh toán với filter và phân trang
    /// </summary>
    Task<PagedResponse<PaymentDetailResponse>> GetPaymentHistoryAsync(
        Guid userId,
        PaymentSearchRequest request);

    /// <summary>
    /// UC30: Lấy chi tiết thanh toán
    /// </summary>
    Task<BaseResponse<PaymentDetailResponse>> GetPaymentByIdAsync(
        Guid userId,
        Guid paymentId);

    /// <summary>
    /// Lấy PayOS payment link và QR code
    /// </summary>
    Task<BaseResponse<PayOSPaymentLinkResponse>> GetPayOSPaymentLinkAsync(
        Guid userId,
        Guid paymentId);
}


