using EVehicle.Application.DTOs.Appointments;
using FluentValidation;

namespace EVehicle.Application.Validators.Appointments;

/// <summary>
/// Validator cho AppointmentUpdateRequest
/// </summary>
public class AppointmentUpdateRequestValidator : AbstractValidator<AppointmentUpdateRequest>
{
    public AppointmentUpdateRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .Must((request, startTime) => !startTime.HasValue || startTime.Value > DateTime.UtcNow)
            .WithMessage("Thời gian bắt đầu phải trong tương lai")
            .When(x => x.StartTime.HasValue);

        RuleFor(x => x.EndTime)
            .Must((request, endTime) => 
                !endTime.HasValue || 
                !request.StartTime.HasValue || 
                endTime.Value > request.StartTime.Value)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu")
            .When(x => x.EndTime.HasValue && x.StartTime.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(500)
            .WithMessage("Địa điểm không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage("Trạng thái không hợp lệ. Các giá trị hợp lệ: CONFIRMED, CANCELED, COMPLETED")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;

        var validStatuses = new[] { "CONFIRMED", "CANCELED", "COMPLETED" };
        return validStatuses.Contains(status.ToUpper());
    }
}

