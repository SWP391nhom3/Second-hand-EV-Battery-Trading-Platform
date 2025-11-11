namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Request DTO cho tạo thanh toán (UC28)
/// </summary>
public class PaymentCreateRequest
{
    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Phương thức thanh toán (PAYOS)
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;
}

