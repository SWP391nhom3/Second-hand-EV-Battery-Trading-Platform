namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Request DTO cho UC31 và UC32: Tạo đánh giá người mua/người bán
/// </summary>
public class RatingCreateRequest
{
    /// <summary>
    /// ID đơn hàng (Order) đã hoàn thành
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Điểm số (1-5 sao)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Nhận xét về sản phẩm, người bán/người mua, giao hàng, thanh toán, giao tiếp
    /// </summary>
    public string? Comment { get; set; }
}


