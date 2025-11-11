using EVehicle.Application.DTOs.Chat;
using FluentValidation;

namespace EVehicle.Application.Validators.Chat;

public class ChatRoomCreateRequestValidator : AbstractValidator<ChatRoomCreateRequest>
{
    public ChatRoomCreateRequestValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("LeadId không được để trống");
    }
}


