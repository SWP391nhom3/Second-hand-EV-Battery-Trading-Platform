using EVehicle.Application.DTOs.Contracts;
using FluentValidation;

namespace EVehicle.Application.Validators.Contracts;

/// <summary>
/// Validator cho ContractSignRequest
/// </summary>
public class ContractSignRequestValidator : AbstractValidator<ContractSignRequest>
{
    public ContractSignRequestValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Chữ ký không được để trống");

        RuleFor(x => x.SignType)
            .NotEmpty().WithMessage("Loại ký không được để trống")
            .Must(signType => new[] { "SIGNATURE", "OTP" }.Contains(signType.ToUpper()))
            .WithMessage("Loại ký không hợp lệ. Chỉ chấp nhận: SIGNATURE, OTP");
    }
}

