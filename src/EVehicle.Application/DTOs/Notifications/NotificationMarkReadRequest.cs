namespace EVehicle.Application.DTOs.Notifications;

/// <summary>
/// Request DTO cho việc đánh dấu thông báo đã đọc (UC38)
/// </summary>
public class NotificationMarkReadRequest
{
    /// <summary>
    /// ID thông báo (nếu null thì đánh dấu tất cả là đã đọc)
    /// </summary>
    public Guid? NotificationId { get; set; }
}

