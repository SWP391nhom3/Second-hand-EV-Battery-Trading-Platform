using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PostCompareRequest
/// </summary>
public class PostCompareRequestValidator : AbstractValidator<PostCompareRequest>
{
    public PostCompareRequestValidator()
    {
        RuleFor(x => x.PostIds)
            .NotEmpty()
            .WithMessage("Danh sách sản phẩm cần so sánh không được để trống")
            .Must(ids => ids.Count >= 2)
            .WithMessage("Phải chọn ít nhất 2 sản phẩm để so sánh")
            .Must(ids => ids.Count <= 5)
            .WithMessage("Chỉ được so sánh tối đa 5 sản phẩm")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Danh sách sản phẩm không được trùng lặp");

        RuleForEach(x => x.PostIds)
            .NotEmpty()
            .WithMessage("ID bài đăng không được để trống");
    }
}


