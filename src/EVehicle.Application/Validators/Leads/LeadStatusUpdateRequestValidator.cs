using EVehicle.Application.DTOs.Leads;
using FluentValidation;

namespace EVehicle.Application.Validators.Leads;

/// <summary>
/// Validator cho LeadStatusUpdateRequest
/// </summary>
public class LeadStatusUpdateRequestValidator : AbstractValidator<LeadStatusUpdateRequest>
{
    public LeadStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống")
            .Must(status => 
            {
                if (string.IsNullOrEmpty(status)) return false;
                var validStatuses = new[] { "CONTACTED", "SCHEDULED", "SUCCESSFUL", "FAILED" };
                return validStatuses.Contains(status.ToUpper());
            })
            .WithMessage("Trạng thái không hợp lệ. Chỉ chấp nhận: CONTACTED, SCHEDULED, SUCCESSFUL, FAILED");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Ghi chú không được vượt quá 2000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

