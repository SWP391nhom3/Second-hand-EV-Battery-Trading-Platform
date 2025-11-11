namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Response DTO cho Rating Reply
/// </summary>
public class RatingReplyResponse
{
    /// <summary>
    /// ID phản hồi
    /// </summary>
    public Guid ReplyId { get; set; }

    /// <summary>
    /// ID đánh giá
    /// </summary>
    public Guid RatingId { get; set; }

    /// <summary>
    /// ID người phản hồi
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tên người phản hồi
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung phản hồi
    /// </summary>
    public string ReplyContent { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}


