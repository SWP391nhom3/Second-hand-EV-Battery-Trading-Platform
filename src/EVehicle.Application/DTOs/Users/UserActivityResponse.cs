namespace EVehicle.Application.DTOs.Users;

/// <summary>
/// Response DTO cho lịch sử hoạt động của người dùng (UC47)
/// </summary>
public class UserActivityResponse
{
    public Guid ActivityId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // POST_CREATED, POST_UPDATED, ORDER_CREATED, etc.
    public string Description { get; set; } = string.Empty;
    public Guid? RelatedId { get; set; } // ID của entity liên quan (post_id, order_id, etc.)
    public DateTime CreatedAt { get; set; }
}

