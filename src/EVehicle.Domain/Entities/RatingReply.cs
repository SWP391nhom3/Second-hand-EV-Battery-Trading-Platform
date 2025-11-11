using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class RatingReply : BaseEntity
{
    public Guid ReplyId => Id;
    public Guid RatingId { get; set; }
    public Guid UserId { get; set; } // Người phản hồi
    public string ReplyContent { get; set; } = string.Empty;

    // Navigation properties
    public Rating Rating { get; set; } = null!;
    public User User { get; set; } = null!;
}

