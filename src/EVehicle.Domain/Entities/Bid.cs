using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Bid : BaseEntity
{
    public Guid BidId => Id;
    public Guid PostId { get; set; }
    public Guid UserId { get; set; } // Người đấu giá
    public decimal BidAmount { get; set; }
    public bool IsWinningBid { get; set; } = false; // Bid thắng

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}

