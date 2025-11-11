using EVehicle.Application.DTOs.Auth;
using FluentValidation;

namespace EVehicle.Application.Validators.Auth;

/// <summary>
/// Validator cho ResendOtpRequest
/// </summary>
public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc")
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự");
    }
}

