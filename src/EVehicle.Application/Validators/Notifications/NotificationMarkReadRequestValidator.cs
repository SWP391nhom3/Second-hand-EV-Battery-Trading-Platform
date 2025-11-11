using EVehicle.Application.DTOs.Notifications;
using FluentValidation;

namespace EVehicle.Application.Validators.Notifications;

/// <summary>
/// Validator cho NotificationMarkReadRequest
/// </summary>
public class NotificationMarkReadRequestValidator : AbstractValidator<NotificationMarkReadRequest>
{
    public NotificationMarkReadRequestValidator()
    {
        // NotificationId có thể null (đánh dấu tất cả là đã đọc)
        // Không cần validation gì thêm
    }
}

