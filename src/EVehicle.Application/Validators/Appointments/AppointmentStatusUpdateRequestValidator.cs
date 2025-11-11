using EVehicle.Application.DTOs.Appointments;
using FluentValidation;

namespace EVehicle.Application.Validators.Appointments;

/// <summary>
/// Validator cho AppointmentStatusUpdateRequest
/// </summary>
public class AppointmentStatusUpdateRequestValidator : AbstractValidator<AppointmentStatusUpdateRequest>
{
    public AppointmentStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Trạng thái không được để trống")
            .Must(BeValidStatus)
            .WithMessage("Trạng thái không hợp lệ. Các giá trị hợp lệ: CONFIRMED, CANCELED, COMPLETED");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeValidStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            return false;

        var validStatuses = new[] { "CONFIRMED", "CANCELED", "COMPLETED" };
        return validStatuses.Contains(status.ToUpper());
    }
}

