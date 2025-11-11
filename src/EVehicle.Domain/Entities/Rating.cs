using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Rating : BaseEntity
{
    public Guid RatingId => Id;
    public Guid OrderId { get; set; } // Giao dịch đã hoàn thành
    public Guid RaterId { get; set; } // Người đánh giá
    public Guid RateeId { get; set; } // Người/Đối tượng bị đánh giá
    public string RateeRole { get; set; } = string.Empty; // SELLER, BUYER
    public int Score { get; set; } // Từ 1 đến 5 sao
    public string? Comment { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public User Rater { get; set; } = null!;
    public User Ratee { get; set; } = null!;
    public ICollection<RatingReply> RatingReplies { get; set; } = new List<RatingReply>();
}

