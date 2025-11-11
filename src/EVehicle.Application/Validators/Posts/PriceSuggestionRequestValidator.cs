using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PriceSuggestionRequest
/// </summary>
public class PriceSuggestionRequestValidator : AbstractValidator<PriceSuggestionRequest>
{
    public PriceSuggestionRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Danh mục không hợp lệ");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Thương hiệu không được để trống")
            .MaximumLength(100).WithMessage("Thương hiệu không được vượt quá 100 ký tự");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model không được để trống")
            .MaximumLength(100).WithMessage("Model không được vượt quá 100 ký tự");

        RuleFor(x => x.BatteryCapacityCurrent)
            .GreaterThan(0).WithMessage("Dung lượng pin phải lớn hơn 0");

        RuleFor(x => x.ProductionYear)
            .InclusiveBetween(2000, DateTime.Now.Year)
            .WithMessage($"Năm sản xuất phải từ 2000 đến {DateTime.Now.Year}");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Tình trạng không được để trống")
            .Must(c => new[] { "Mới", "Cũ", "Đã qua sử dụng" }.Contains(c))
            .WithMessage("Tình trạng phải là: Mới, Cũ, hoặc Đã qua sử dụng");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Địa điểm không được để trống")
            .MaximumLength(255).WithMessage("Địa điểm không được vượt quá 255 ký tự");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue)
            .WithMessage("Số KM phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.ChargeCount)
            .GreaterThanOrEqualTo(0).When(x => x.ChargeCount.HasValue)
            .WithMessage("Số lần sạc phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.Description)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Mô tả không được vượt quá 5000 ký tự");
    }
}


