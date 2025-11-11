namespace EVehicle.Application.DTOs.Bids;

/// <summary>
/// Response DTO cho bid
/// </summary>
public class BidResponse
{
    /// <summary>
    /// ID bid
    /// </summary>
    public Guid BidId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// ID người đấu giá
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tên người đấu giá
    /// </summary>
    public string BidderName { get; set; } = string.Empty;

    /// <summary>
    /// Giá đấu
    /// </summary>
    public decimal BidAmount { get; set; }

    /// <summary>
    /// Là bid thắng
    /// </summary>
    public bool IsWinningBid { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

