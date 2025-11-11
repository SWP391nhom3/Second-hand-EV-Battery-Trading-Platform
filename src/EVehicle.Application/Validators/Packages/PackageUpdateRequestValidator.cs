using EVehicle.Application.DTOs.Packages;
using FluentValidation;

namespace EVehicle.Application.Validators.Packages;

/// <summary>
/// Validator cho PackageUpdateRequest
/// </summary>
public class PackageUpdateRequestValidator : AbstractValidator<PackageUpdateRequest>
{
    public PackageUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên gói tin không được để trống")
            .MaximumLength(50).WithMessage("Tên gói tin không được vượt quá 50 ký tự")
            .Matches("^[a-zA-Z0-9\\s]+$").WithMessage("Tên gói tin chỉ được chứa chữ cái, số và khoảng trắng");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá phải lớn hơn 0")
            .LessThanOrEqualTo(1000000000).WithMessage("Giá không được vượt quá 1,000,000,000 VNĐ");

        RuleFor(x => x.CreditsCount)
            .GreaterThan(0).WithMessage("Số credits phải lớn hơn 0")
            .LessThanOrEqualTo(1000).WithMessage("Số credits không được vượt quá 1000");

        RuleFor(x => x.PriorityLevel)
            .InclusiveBetween(1, 10).WithMessage("Mức độ ưu tiên phải từ 1 đến 10");

        RuleFor(x => x.MaxImages)
            .GreaterThan(0).WithMessage("Số ảnh tối đa phải lớn hơn 0")
            .LessThanOrEqualTo(50).WithMessage("Số ảnh tối đa không được vượt quá 50");
    }
}


