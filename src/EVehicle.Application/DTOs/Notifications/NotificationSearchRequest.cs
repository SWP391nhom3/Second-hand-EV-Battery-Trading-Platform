using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Notifications;

/// <summary>
/// Request DTO cho việc tìm kiếm thông báo (UC37)
/// </summary>
public class NotificationSearchRequest : PagedRequest
{
    /// <summary>
    /// Lọc theo loại thông báo (NEW_MESSAGE, NEW_BID, ORDER_UPDATE, etc.)
    /// </summary>
    public string? NotificationType { get; set; }

    /// <summary>
    /// Lọc theo trạng thái đã đọc (true = chỉ lấy đã đọc, false = chỉ lấy chưa đọc, null = lấy tất cả)
    /// </summary>
    public bool? IsRead { get; set; }
}

