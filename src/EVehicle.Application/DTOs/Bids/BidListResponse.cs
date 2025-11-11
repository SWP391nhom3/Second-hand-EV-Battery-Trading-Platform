namespace EVehicle.Application.DTOs.Bids;

/// <summary>
/// Response DTO cho danh sách bids của một bài đăng
/// </summary>
public class BidListResponse
{
    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Giá khởi điểm
    /// </summary>
    public decimal? StartingBid { get; set; }

    /// <summary>
    /// Giá mua ngay
    /// </summary>
    public decimal? BuyNowPrice { get; set; }

    /// <summary>
    /// Thời gian kết thúc đấu giá
    /// </summary>
    public DateTime? AuctionEndTime { get; set; }

    /// <summary>
    /// Giá cao nhất hiện tại
    /// </summary>
    public decimal? CurrentHighestBid { get; set; }

    /// <summary>
    /// Tổng số bids
    /// </summary>
    public int TotalBids { get; set; }

    /// <summary>
    /// Danh sách bids
    /// </summary>
    public List<BidResponse> Bids { get; set; } = new();
}

