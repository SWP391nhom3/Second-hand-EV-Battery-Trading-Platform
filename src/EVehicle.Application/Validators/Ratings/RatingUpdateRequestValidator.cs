using EVehicle.Application.DTOs.Ratings;
using FluentValidation;

namespace EVehicle.Application.Validators.Ratings;

/// <summary>
/// Validator cho RatingUpdateRequest
/// </summary>
public class RatingUpdateRequestValidator : AbstractValidator<RatingUpdateRequest>
{
    public RatingUpdateRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Điểm số phải từ 1 đến 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Nhận xét không được vượt quá 2000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}


