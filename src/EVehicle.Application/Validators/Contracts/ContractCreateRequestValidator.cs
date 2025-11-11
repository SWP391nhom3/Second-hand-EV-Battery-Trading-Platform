using EVehicle.Application.DTOs.Contracts;
using FluentValidation;

namespace EVehicle.Application.Validators.Contracts;

/// <summary>
/// Validator cho ContractCreateRequest
/// </summary>
public class ContractCreateRequestValidator : AbstractValidator<ContractCreateRequest>
{
    public ContractCreateRequestValidator()
    {
        RuleFor(x => x.ContractTemplateId)
            .GreaterThan(0).WithMessage("ID mẫu hợp đồng không hợp lệ");

        RuleFor(x => x)
            .Must(x => x.OrderId.HasValue || x.LeadId.HasValue)
            .WithMessage("Phải có OrderId hoặc LeadId");

        RuleFor(x => x.ContractContent)
            .MaximumLength(50000).WithMessage("Nội dung hợp đồng không được vượt quá 50000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ContractContent));
    }
}

