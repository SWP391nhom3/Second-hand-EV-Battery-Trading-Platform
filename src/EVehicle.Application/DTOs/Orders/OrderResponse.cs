namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Response DTO cho Order
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// ID Lead (nếu có)
    /// </summary>
    public Guid? LeadId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string PostTitle { get; set; } = string.Empty;

    /// <summary>
    /// ID người mua
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Tên người mua
    /// </summary>
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>
    /// ID người bán
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// ID Staff hỗ trợ (nếu có)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Tên Staff
    /// </summary>
    public string? StaffName { get; set; }

    /// <summary>
    /// Giá cuối cùng
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng (PENDING_PAYMENT, PAID, CONFIRMED, SHIPPING, DELIVERED, COMPLETED, CANCELLED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Địa chỉ giao hàng
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Thời gian thanh toán
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Thời gian hoàn thành
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

