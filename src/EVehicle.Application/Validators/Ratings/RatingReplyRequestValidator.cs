using EVehicle.Application.DTOs.Ratings;
using FluentValidation;

namespace EVehicle.Application.Validators.Ratings;

/// <summary>
/// Validator cho RatingReplyRequest
/// </summary>
public class RatingReplyRequestValidator : AbstractValidator<RatingReplyRequest>
{
    public RatingReplyRequestValidator()
    {
        RuleFor(x => x.ReplyContent)
            .NotEmpty().WithMessage("Nội dung phản hồi không được để trống")
            .MaximumLength(1000).WithMessage("Nội dung phản hồi không được vượt quá 1000 ký tự");
    }
}


