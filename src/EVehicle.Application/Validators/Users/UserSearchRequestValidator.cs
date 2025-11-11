using EVehicle.Application.DTOs.Users;
using FluentValidation;

namespace EVehicle.Application.Validators.Users;

/// <summary>
/// Validator cho UserSearchRequest
/// </summary>
public class UserSearchRequestValidator : AbstractValidator<UserSearchRequest>
{
    public UserSearchRequestValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrEmpty(role) || 
                role == "MEMBER" || role == "STAFF" || role == "ADMIN")
            .WithMessage("Role phải là MEMBER, STAFF hoặc ADMIN");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || 
                status == "ACTIVE" || status == "BANNED" || 
                status == "SUSPENDED" || status == "PENDING_VERIFICATION")
            .WithMessage("Status phải là ACTIVE, BANNED, SUSPENDED hoặc PENDING_VERIFICATION");
    }
}

