namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Response DTO cho Payment
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// ID thanh toán
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// ID người dùng
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Số tiền
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Cổng thanh toán (PAYOS)
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;

    /// <summary>
    /// Mã giao dịch từ cổng thanh toán
    /// </summary>
    public string? TransactionCode { get; set; }

    /// <summary>
    /// Trạng thái (PENDING, SUCCESS, FAILED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Loại thanh toán (PACKAGE, TRANSACTION)
    /// </summary>
    public string PaymentType { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hoàn tất
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// URL thanh toán (nếu có)
    /// </summary>
    public string? PaymentUrl { get; set; }
}

