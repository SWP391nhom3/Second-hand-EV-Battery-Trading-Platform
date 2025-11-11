using EVehicle.Application.DTOs.Users;
using FluentValidation;

namespace EVehicle.Application.Validators.Users;

/// <summary>
/// Validator cho UserUpdateRequest
/// </summary>
public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrEmpty(role) || 
                role == "MEMBER" || role == "STAFF" || role == "ADMIN")
            .WithMessage("Role phải là MEMBER, STAFF hoặc ADMIN")
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || 
                status == "ACTIVE" || status == "BANNED" || 
                status == "SUSPENDED" || status == "PENDING_VERIFICATION")
            .WithMessage("Status phải là ACTIVE, BANNED, SUSPENDED hoặc PENDING_VERIFICATION")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Role) || !string.IsNullOrEmpty(x.Status))
            .WithMessage("Phải cung cấp ít nhất một trong hai: Role hoặc Status");
    }
}

