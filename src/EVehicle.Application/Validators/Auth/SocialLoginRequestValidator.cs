using EVehicle.Application.DTOs.Auth;
using FluentValidation;

namespace EVehicle.Application.Validators.Auth;

/// <summary>
/// Validator cho SocialLoginRequest (UC03)
/// </summary>
public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequest>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider không được để trống")
            .Must(x => x.ToLower() == "google" || x.ToLower() == "facebook")
            .WithMessage("Provider phải là 'google' hoặc 'facebook'");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token không được để trống");
    }
}

