using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Notification entity
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly EVehicleDbContext _context;

    public NotificationRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<PagedResult<Notification>> GetNotificationsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? notificationType,
        bool? isRead,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(notificationType))
        {
            query = query.Where(n => n.NotificationType == notificationType);
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            // Mặc định sắp xếp theo thời gian tạo (mới nhất trước)
            query = query.OrderByDescending(n => n.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Notification>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification != null)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        _context.Notifications.UpdateRange(unreadNotifications);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private static Expression<Func<Notification, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "createdat" => n => n.CreatedAt,
            "isread" => n => n.IsRead,
            "notificationtype" => n => n.NotificationType,
            _ => n => n.CreatedAt
        };
    }
}

