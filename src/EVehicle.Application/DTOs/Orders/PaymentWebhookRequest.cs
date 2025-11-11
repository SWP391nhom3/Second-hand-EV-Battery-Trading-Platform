namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Request DTO cho webhook thanh toán
/// </summary>
public class PaymentWebhookRequest
{
    /// <summary>
    /// Mã giao dịch từ cổng thanh toán
    /// </summary>
    public string TransactionCode { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Số tiền
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Cổng thanh toán
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu bổ sung từ cổng thanh toán
    /// </summary>
    public Dictionary<string, string>? AdditionalData { get; set; }

    /// <summary>
    /// PaymentId (nếu cổng thanh toán trả về)
    /// </summary>
    public Guid? PaymentId { get; set; }
}

