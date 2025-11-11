using EVehicle.Application.DTOs.Leads;
using FluentValidation;

namespace EVehicle.Application.Validators.Leads;

/// <summary>
/// Validator cho LeadAssignStaffRequest
/// </summary>
public class LeadAssignStaffRequestValidator : AbstractValidator<LeadAssignStaffRequest>
{
    public LeadAssignStaffRequestValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("StaffId không được để trống")
            .NotEqual(Guid.Empty).WithMessage("StaffId không hợp lệ");
    }
}

