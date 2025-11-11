using EVehicle.Application.DTOs.Orders;
using FluentValidation;

namespace EVehicle.Application.Validators.Orders;

/// <summary>
/// Validator cho PaymentCreateRequest
/// </summary>
public class PaymentCreateRequestValidator : AbstractValidator<PaymentCreateRequest>
{
    public PaymentCreateRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("ID đơn hàng không được để trống");

        RuleFor(x => x.PaymentGateway)
            .NotEmpty().WithMessage("Phương thức thanh toán không được để trống")
            .Must(gateway => new[] { "PAYOS" }.Contains(gateway.ToUpper()))
            .WithMessage("Phương thức thanh toán không hợp lệ. Chỉ chấp nhận: PAYOS");
    }
}

