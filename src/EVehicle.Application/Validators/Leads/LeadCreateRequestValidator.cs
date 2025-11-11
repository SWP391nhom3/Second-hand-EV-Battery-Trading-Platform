using EVehicle.Application.DTOs.Leads;
using FluentValidation;

namespace EVehicle.Application.Validators.Leads;

/// <summary>
/// Validator cho LeadCreateRequest
/// </summary>
public class LeadCreateRequestValidator : AbstractValidator<LeadCreateRequest>
{
    public LeadCreateRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("ID bài đăng không được để trống");

        RuleFor(x => x.LeadType)
            .NotEmpty().WithMessage("Loại Lead không được để trống")
            .Must(leadType => leadType == "SCHEDULE_VIEW" || leadType == "AUCTION_WINNER")
            .WithMessage("Loại Lead không hợp lệ. Chỉ chấp nhận SCHEDULE_VIEW hoặc AUCTION_WINNER");
    }
}

