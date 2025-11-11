using EVehicle.Application.DTOs.Users;
using FluentValidation;

namespace EVehicle.Application.Validators.Users;

/// <summary>
/// Validator cho UpdateProfileRequest (UC04)
/// </summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        // FullName validation (optional, but if provided must be valid)
        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .WithMessage("Họ tên không được vượt quá 100 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        // Address validation (optional, but if provided must be valid)
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        // AvatarUrl validation (optional, but if provided must be valid URL)
        RuleFor(x => x.AvatarUrl)
            .Must(BeValidUrl)
            .WithMessage("URL ảnh đại diện không hợp lệ")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        // IdNumber validation (optional, but if provided must be valid format)
        RuleFor(x => x.IdNumber)
            .Matches(@"^[0-9]{9,12}$")
            .WithMessage("Số CMND/CCCD phải có 9-12 chữ số")
            .When(x => !string.IsNullOrWhiteSpace(x.IdNumber));

        // At least one field must be provided
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("Vui lòng cập nhật ít nhất một trường thông tin");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private bool HaveAtLeastOneField(UpdateProfileRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FullName) ||
               !string.IsNullOrWhiteSpace(request.Address) ||
               !string.IsNullOrWhiteSpace(request.AvatarUrl) ||
               !string.IsNullOrWhiteSpace(request.IdNumber);
    }
}


