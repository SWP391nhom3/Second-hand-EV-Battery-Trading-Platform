using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Notification Repository
/// </summary>
public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    
    /// <summary>
    /// Lấy thông báo theo ID
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid notificationId);
    
    /// <summary>
    /// Lấy danh sách thông báo của user (có phân trang và filter)
    /// </summary>
    Task<PagedResult<Notification>> GetNotificationsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? notificationType,
        bool? isRead,
        string? sortBy,
        string? sortDirection);
    
    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId);
    
    /// <summary>
    /// Đánh dấu tất cả thông báo của user là đã đọc
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId);
    
    /// <summary>
    /// Đếm số thông báo chưa đọc của user
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId);
    
    Task SaveChangesAsync();
}

