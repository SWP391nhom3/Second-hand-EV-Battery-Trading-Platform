namespace EVehicle.Application.DTOs.Bids;

/// <summary>
/// Request DTO cho việc đặt giá đấu
/// </summary>
public class BidCreateRequest
{
    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Giá đấu (phải cao hơn giá hiện tại)
    /// </summary>
    public decimal BidAmount { get; set; }
}

