namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Request DTO cho UC34: Phản hồi đánh giá
/// </summary>
public class RatingReplyRequest
{
    /// <summary>
    /// Nội dung phản hồi
    /// </summary>
    public string ReplyContent { get; set; } = string.Empty;
}


