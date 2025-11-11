using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PostCreateRequest
/// </summary>
public class PostCreateRequestValidator : AbstractValidator<PostCreateRequest>
{
    public PostCreateRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Danh mục không hợp lệ");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề không được để trống")
            .MaximumLength(255)
            .WithMessage("Tiêu đề không được vượt quá 255 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Mô tả không được để trống")
            .MaximumLength(5000)
            .WithMessage("Mô tả không được vượt quá 5000 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Giá phải lớn hơn 0")
            .LessThanOrEqualTo(999999999999.99m)
            .WithMessage("Giá không được vượt quá 999,999,999,999.99");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Địa điểm không được để trống")
            .MaximumLength(500)
            .WithMessage("Địa điểm không được vượt quá 500 ký tự");

        RuleFor(x => x.Brand)
            .NotEmpty()
            .WithMessage("Thương hiệu không được để trống")
            .MaximumLength(100)
            .WithMessage("Thương hiệu không được vượt quá 100 ký tự");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Model không được để trống")
            .MaximumLength(100)
            .WithMessage("Model không được vượt quá 100 ký tự");

        RuleFor(x => x.BatteryCapacityCurrent)
            .GreaterThan(0)
            .WithMessage("Dung lượng pin phải lớn hơn 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Dung lượng pin không hợp lệ");

        RuleFor(x => x.ChargeCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ChargeCount.HasValue)
            .WithMessage("Số lần sạc phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.ProductionYear)
            .InclusiveBetween(2000, DateTime.Now.Year)
            .WithMessage($"Năm sản xuất phải từ 2000 đến {DateTime.Now.Year}");

        RuleFor(x => x.Condition)
            .NotEmpty()
            .WithMessage("Tình trạng không được để trống")
            .MaximumLength(50)
            .WithMessage("Tình trạng không được vượt quá 50 ký tự");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Mileage.HasValue)
            .WithMessage("Số KM phải lớn hơn hoặc bằng 0")
            .LessThanOrEqualTo(1000000)
            .When(x => x.Mileage.HasValue)
            .WithMessage("Số KM không hợp lệ");

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage("Phải có ít nhất 1 ảnh sản phẩm")
            .Must(images => images != null && images.Count <= 10)
            .WithMessage("Tối đa 10 ảnh sản phẩm");

        RuleForEach(x => x.Images)
            .Must(BeValidImage)
            .WithMessage("File ảnh không hợp lệ. Chỉ chấp nhận file JPG, PNG, JPEG với kích thước tối đa 5MB");

        RuleFor(x => x.ProofImage)
            .NotNull()
            .WithMessage("Ảnh bằng chứng SOH/KM là bắt buộc")
            .Must(BeValidImage)
            .When(x => x.ProofImage != null)
            .WithMessage("File ảnh bằng chứng không hợp lệ. Chỉ chấp nhận file JPG, PNG, JPEG với kích thước tối đa 5MB");

        RuleFor(x => x.PackageId)
            .GreaterThan(0)
            .WithMessage("Gói tin không hợp lệ");

        // Auction validation
        RuleFor(x => x.StartingBid)
            .GreaterThan(0)
            .When(x => x.AuctionEnabled)
            .WithMessage("Giá khởi điểm đấu giá phải lớn hơn 0 khi bật chế độ đấu giá");

        RuleFor(x => x.AuctionEndTime)
            .NotNull()
            .When(x => x.AuctionEnabled)
            .WithMessage("Thời gian kết thúc đấu giá là bắt buộc khi bật chế độ đấu giá")
            .Must(endTime => endTime.HasValue && endTime.Value > DateTime.UtcNow)
            .When(x => x.AuctionEnabled)
            .WithMessage("Thời gian kết thúc đấu giá phải lớn hơn thời gian hiện tại");

        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.StartingBid ?? 0)
            .When(x => x.AuctionEnabled && x.BuyNowPrice.HasValue && x.StartingBid.HasValue)
            .WithMessage("Giá mua ngay phải lớn hơn giá khởi điểm");

        RuleFor(x => x.StartingBid)
            .Null()
            .When(x => !x.AuctionEnabled)
            .WithMessage("Không được nhập giá khởi điểm khi chưa bật chế độ đấu giá");

        RuleFor(x => x.BuyNowPrice)
            .Null()
            .When(x => !x.AuctionEnabled)
            .WithMessage("Không được nhập giá mua ngay khi chưa bật chế độ đấu giá");

        RuleFor(x => x.AuctionEndTime)
            .Null()
            .When(x => !x.AuctionEnabled)
            .WithMessage("Không được nhập thời gian kết thúc khi chưa bật chế độ đấu giá");
    }

    private bool BeValidImage(FileUploadDto? file)
    {
        if (file == null)
            return false;

        // Kiểm tra extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return false;

        // Kiểm tra kích thước (5MB)
        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
            return false;

        // Kiểm tra content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }
}

