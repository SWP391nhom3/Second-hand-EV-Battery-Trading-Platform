namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Request DTO cho tạo đơn hàng (UC28)
/// </summary>
public class OrderCreateRequest
{
    /// <summary>
    /// ID Lead (nếu có)
    /// </summary>
    public Guid? LeadId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// ID người mua
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Giá cuối cùng
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Địa chỉ giao hàng (nếu có)
    /// </summary>
    public string? ShippingAddress { get; set; }
}

