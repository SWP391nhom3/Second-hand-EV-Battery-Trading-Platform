using EVehicle.Application.DTOs.Auth;
using FluentValidation;

namespace EVehicle.Application.Validators.Auth;

/// <summary>
/// Validator cho LoginRequest (UC02)
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrPhone)
            .NotEmpty().WithMessage("Email hoặc Số điện thoại không được để trống")
            .MaximumLength(255).WithMessage("Email hoặc Số điện thoại không được vượt quá 255 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống");
    }
}

