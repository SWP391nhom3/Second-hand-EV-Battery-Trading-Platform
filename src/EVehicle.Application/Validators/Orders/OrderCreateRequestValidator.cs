using EVehicle.Application.DTOs.Orders;
using FluentValidation;

namespace EVehicle.Application.Validators.Orders;

/// <summary>
/// Validator cho OrderCreateRequest
/// </summary>
public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("ID bài đăng không được để trống");

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("ID người mua không được để trống");

        RuleFor(x => x.FinalPrice)
            .GreaterThan(0).WithMessage("Giá cuối cùng phải lớn hơn 0");

        RuleFor(x => x.ShippingAddress)
            .MaximumLength(500).WithMessage("Địa chỉ giao hàng không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ShippingAddress));
    }
}

