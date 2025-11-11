namespace EVehicle.Application.DTOs.Notifications;

/// <summary>
/// Response DTO cho thông báo
/// </summary>
public class NotificationResponse
{
    /// <summary>
    /// ID thông báo
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Loại thông báo (NEW_MESSAGE, NEW_BID, ORDER_UPDATE, PRICE_CHANGE, NEW_LEAD, APPOINTMENT, etc.)
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Tiêu đề thông báo
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung thông báo
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ID liên quan (post_id, order_id, room_id, etc.)
    /// </summary>
    public Guid? RelatedId { get; set; }

    /// <summary>
    /// Đã đọc chưa
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

