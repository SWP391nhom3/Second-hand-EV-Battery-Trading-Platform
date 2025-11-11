using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid NotificationId => Id;
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; 
    // NEW_MESSAGE, NEW_BID, ORDER_UPDATE, PRICE_CHANGE, NEW_LEAD, APPOINTMENT, etc.
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? RelatedId { get; set; } // ID liên quan (post_id, order_id, etc.)
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
}

