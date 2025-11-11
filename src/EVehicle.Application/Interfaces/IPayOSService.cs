using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho PayOS Service
/// </summary>
public interface IPayOSService
{
    /// <summary>
    /// Tạo payment link từ PayOS và trả về QR code URL
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng (sử dụng paymentId)</param>
    /// <param name="amount">Số tiền thanh toán (đơn vị: VND)</param>
    /// <param name="description">Mô tả đơn hàng</param>
    /// <param name="returnUrl">URL redirect sau khi thanh toán thành công</param>
    /// <param name="cancelUrl">URL redirect khi hủy thanh toán</param>
    /// <returns>BaseResponse chứa paymentUrl và qrCodeUrl</returns>
    Task<BaseResponse<PayOSPaymentLinkResponse>> CreatePaymentLinkAsync(
        long orderCode,
        decimal amount,
        string description,
        string? returnUrl = null,
        string? cancelUrl = null);
}

/// <summary>
/// Response DTO cho PayOS Payment Link
/// </summary>
public class PayOSPaymentLinkResponse
{
    /// <summary>
    /// Payment URL từ PayOS
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>
    /// QR Code URL từ PayOS
    /// </summary>
    public string QrCodeUrl { get; set; } = string.Empty;

    /// <summary>
    /// Order Code
    /// </summary>
    public long OrderCode { get; set; }

    /// <summary>
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }
}

