using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Request DTO cho tìm kiếm đánh giá
/// </summary>
public class RatingSearchRequest : PagedRequest
{
    /// <summary>
    /// ID người được đánh giá (để xem đánh giá về một người)
    /// </summary>
    public Guid? RateeId { get; set; }

    /// <summary>
    /// ID đơn hàng (để xem đánh giá của một đơn hàng)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Vai trò người được đánh giá (SELLER, BUYER)
    /// </summary>
    public string? RateeRole { get; set; }

    /// <summary>
    /// Điểm số tối thiểu (1-5)
    /// </summary>
    public int? MinScore { get; set; }

    /// <summary>
    /// Điểm số tối đa (1-5)
    /// </summary>
    public int? MaxScore { get; set; }
}


