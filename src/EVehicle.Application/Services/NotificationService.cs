using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Notifications;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Notification Service implementation
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(
        Guid userId, 
        NotificationSearchRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return PagedResponse<NotificationResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            request.IsValid();

            // 2. Lấy danh sách thông báo
            var result = await _notificationRepository.GetNotificationsByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                request.NotificationType,
                request.IsRead,
                request.SortBy,
                request.SortDirection);

            // 3. Map to response
            var responses = result.Items.Select(MapToNotificationResponse).ToList();

            return PagedResponse<NotificationResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách thông báo thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo, UserId: {UserId}", userId);
            return PagedResponse<NotificationResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách thông báo");
        }
    }

    public async Task<BaseResponse> MarkNotificationAsReadAsync(
        Guid userId, 
        NotificationMarkReadRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse.FailureResponse("Người dùng không tồn tại");
            }

            if (request.NotificationId.HasValue)
            {
                // Đánh dấu một thông báo cụ thể là đã đọc
                var notification = await _notificationRepository.GetByIdAsync(
                    request.NotificationId.Value);
                
                if (notification == null)
                {
                    return BaseResponse.FailureResponse("Thông báo không tồn tại");
                }

                if (notification.UserId != userId)
                {
                    return BaseResponse.FailureResponse(
                        "Bạn không có quyền đánh dấu thông báo này");
                }

                await _notificationRepository.MarkAsReadAsync(request.NotificationId.Value);
                await _notificationRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Đánh dấu thông báo đã đọc, NotificationId: {NotificationId}, UserId: {UserId}",
                    request.NotificationId.Value, userId);

                return BaseResponse.SuccessResponse("Đánh dấu thông báo đã đọc thành công");
            }
            else
            {
                // Đánh dấu tất cả thông báo là đã đọc
                await _notificationRepository.MarkAllAsReadAsync(userId);
                await _notificationRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Đánh dấu tất cả thông báo đã đọc, UserId: {UserId}", userId);

                return BaseResponse.SuccessResponse("Đánh dấu tất cả thông báo đã đọc thành công");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc, UserId: {UserId}", userId);
            return BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh dấu thông báo đã đọc");
        }
    }

    public async Task<BaseResponse<int>> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<int>.FailureResponse("Người dùng không tồn tại");
            }

            // 2. Đếm số thông báo chưa đọc
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

            return BaseResponse<int>.SuccessResponse(
                unreadCount,
                "Lấy số thông báo chưa đọc thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy số thông báo chưa đọc, UserId: {UserId}", userId);
            return BaseResponse<int>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy số thông báo chưa đọc");
        }
    }

    /// <summary>
    /// Tạo thông báo mới (helper method)
    /// </summary>
    public async Task CreateNotificationAsync(
        Guid userId,
        string notificationType,
        string title,
        string content,
        Guid? relatedId = null)
    {
        try
        {
            var notification = new Domain.Entities.Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = notificationType,
                Title = title,
                Content = content,
                RelatedId = relatedId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Đã tạo thông báo, UserId: {UserId}, Type: {NotificationType}, RelatedId: {RelatedId}",
                userId,
                notificationType,
                relatedId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Lỗi khi tạo thông báo, UserId: {UserId}, Type: {NotificationType}",
                userId,
                notificationType);
            // Không throw exception để không ảnh hưởng đến flow chính
        }
    }

    private NotificationResponse MapToNotificationResponse(Domain.Entities.Notification notification)
    {
        return new NotificationResponse
        {
            NotificationId = notification.Id,
            NotificationType = notification.NotificationType,
            Title = notification.Title,
            Content = notification.Content,
            RelatedId = notification.RelatedId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}

