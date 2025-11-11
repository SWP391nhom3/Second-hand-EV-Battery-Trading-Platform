using EVehicle.Application.DTOs.Packages;
using FluentValidation;

namespace EVehicle.Application.Validators.Packages;

/// <summary>
/// Validator cho PackagePurchaseRequest
/// </summary>
public class PackagePurchaseRequestValidator : AbstractValidator<PackagePurchaseRequest>
{
    public PackagePurchaseRequestValidator()
    {
        RuleFor(x => x.PackageId)
            .GreaterThan(0)
            .WithMessage("Gói tin không hợp lệ");

        RuleFor(x => x.PaymentGateway)
            .NotEmpty()
            .WithMessage("Phương thức thanh toán không được để trống")
            .Must(gateway => gateway.ToUpper() == "PAYOS")
            .WithMessage("Phương thức thanh toán không hợp lệ. Chỉ chấp nhận PAYOS");
    }
}

