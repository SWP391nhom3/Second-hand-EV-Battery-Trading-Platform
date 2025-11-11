using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PostRejectRequest
/// </summary>
public class PostRejectRequestValidator : AbstractValidator<PostRejectRequest>
{
    public PostRejectRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Lý do từ chối không được để trống")
            .MinimumLength(10).WithMessage("Lý do từ chối phải có ít nhất 10 ký tự")
            .MaximumLength(1000).WithMessage("Lý do từ chối không được vượt quá 1000 ký tự");
    }
}

