using EVehicle.Application.DTOs.Appointments;
using FluentValidation;

namespace EVehicle.Application.Validators.Appointments;

/// <summary>
/// Validator cho AppointmentCreateRequest
/// </summary>
public class AppointmentCreateRequestValidator : AbstractValidator<AppointmentCreateRequest>
{
    public AppointmentCreateRequestValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("LeadId không được để trống");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Thời gian bắt đầu không được để trống")
            .Must(BeInFuture)
            .WithMessage("Thời gian bắt đầu phải trong tương lai")
            .When(x => x.StartTime != default);

        RuleFor(x => x.EndTime)
            .Must((request, endTime) => !endTime.HasValue || endTime.Value > request.StartTime)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu")
            .When(x => x.EndTime.HasValue);

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Địa điểm không được để trống")
            .MaximumLength(500)
            .WithMessage("Địa điểm không được vượt quá 500 ký tự");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeInFuture(DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }
}

