using EVehicle.Application.DTOs.Auth;
using FluentValidation;

namespace EVehicle.Application.Validators.Auth;

/// <summary>
/// Validator cho VerifyEmailRequest
/// </summary>
public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc")
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Mã OTP là bắt buộc")
            .Length(6).WithMessage("Mã OTP phải có 6 số")
            .Matches(@"^\d{6}$").WithMessage("Mã OTP chỉ được chứa số");
    }
}

