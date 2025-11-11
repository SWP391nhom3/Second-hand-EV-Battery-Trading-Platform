using EVehicle.Application.DTOs.Chat;
using FluentValidation;

namespace EVehicle.Application.Validators.Chat;

/// <summary>
/// Validator cho ChatHistoryRequest
/// </summary>
public class ChatHistoryRequestValidator : AbstractValidator<ChatHistoryRequest>
{
    public ChatHistoryRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("ID phòng chat không được để trống");
    }
}

