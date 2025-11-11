namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Response DTO chi tiết cho Payment (UC30)
/// </summary>
public class PaymentDetailResponse
{
    /// <summary>
    /// ID thanh toán
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// ID đơn hàng (nếu là TRANSACTION)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// ID gói tin (nếu là PACKAGE)
    /// </summary>
    public int? PackageId { get; set; }

    /// <summary>
    /// Tên gói tin (nếu là PACKAGE)
    /// </summary>
    public string? PackageName { get; set; }

    /// <summary>
    /// Số credits (nếu là PACKAGE)
    /// </summary>
    public int? CreditsCount { get; set; }

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
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian hoàn tất
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thông tin đơn hàng (nếu là TRANSACTION)
    /// </summary>
    public OrderInfo? OrderInfo { get; set; }
}

/// <summary>
/// Thông tin đơn hàng (cho PaymentDetailResponse)
/// </summary>
public class OrderInfo
{
    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string PostTitle { get; set; } = string.Empty;

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Giá cuối cùng
    /// </summary>
    public decimal FinalPrice { get; set; }
}


