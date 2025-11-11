using EVehicle.Application.DTOs.Bids;
using FluentValidation;

namespace EVehicle.Application.Validators.Bids;

/// <summary>
/// Validator cho BidCreateRequest
/// </summary>
public class BidCreateRequestValidator : AbstractValidator<BidCreateRequest>
{
    public BidCreateRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("ID bài đăng không được để trống");

        RuleFor(x => x.BidAmount)
            .GreaterThan(0).WithMessage("Giá đấu phải lớn hơn 0")
            .PrecisionScale(15, 2, false).WithMessage("Giá đấu không hợp lệ (tối đa 15 chữ số, 2 chữ số thập phân)");
    }
}

