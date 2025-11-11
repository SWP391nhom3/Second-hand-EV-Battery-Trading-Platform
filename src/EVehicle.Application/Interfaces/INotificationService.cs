using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Notifications;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Notification Service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// UC37: Xem danh sách thông báo
    /// </summary>
    Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(
        Guid userId, 
        NotificationSearchRequest request);

    /// <summary>
    /// UC38: Đánh dấu thông báo đã đọc
    /// </summary>
    Task<BaseResponse> MarkNotificationAsReadAsync(
        Guid userId, 
        NotificationMarkReadRequest request);

    /// <summary>
    /// Lấy số thông báo chưa đọc
    /// </summary>
    Task<BaseResponse<int>> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// Tạo thông báo mới (helper method)
    /// </summary>
    Task CreateNotificationAsync(
        Guid userId,
        string notificationType,
        string title,
        string content,
        Guid? relatedId = null);
}

