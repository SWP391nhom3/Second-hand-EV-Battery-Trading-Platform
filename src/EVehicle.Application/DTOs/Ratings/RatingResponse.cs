namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Response DTO cho Rating
/// </summary>
public class RatingResponse
{
    /// <summary>
    /// ID đánh giá
    /// </summary>
    public Guid RatingId { get; set; }

    /// <summary>
    /// ID đơn hàng
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// ID người đánh giá
    /// </summary>
    public Guid RaterId { get; set; }

    /// <summary>
    /// Tên người đánh giá
    /// </summary>
    public string RaterName { get; set; } = string.Empty;

    /// <summary>
    /// ID người được đánh giá
    /// </summary>
    public Guid RateeId { get; set; }

    /// <summary>
    /// Tên người được đánh giá
    /// </summary>
    public string RateeName { get; set; } = string.Empty;

    /// <summary>
    /// Vai trò người được đánh giá (SELLER, BUYER)
    /// </summary>
    public string RateeRole { get; set; } = string.Empty;

    /// <summary>
    /// Điểm số (1-5 sao)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Nhận xét
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Danh sách phản hồi
    /// </summary>
    public List<RatingReplyResponse> Replies { get; set; } = new();

    /// <summary>
    /// Có thể chỉnh sửa không (trong vòng 7 ngày)
    /// </summary>
    public bool CanEdit { get; set; }
}


