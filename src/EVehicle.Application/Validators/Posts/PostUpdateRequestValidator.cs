using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PostUpdateRequest
/// </summary>
public class PostUpdateRequestValidator : AbstractValidator<PostUpdateRequest>
{
    public PostUpdateRequestValidator()
    {
        // Title validation (optional, but if provided must be valid)
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title != null)
            .WithMessage("Tiêu đề không được để trống")
            .MaximumLength(255)
            .When(x => x.Title != null)
            .WithMessage("Tiêu đề không được vượt quá 255 ký tự");

        // Description validation (optional, but if provided must be valid)
        RuleFor(x => x.Description)
            .NotEmpty()
            .When(x => x.Description != null)
            .WithMessage("Mô tả không được để trống")
            .MaximumLength(5000)
            .When(x => x.Description != null)
            .WithMessage("Mô tả không được vượt quá 5000 ký tự");

        // Price validation (optional, but if provided must be valid)
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Giá phải lớn hơn 0")
            .LessThanOrEqualTo(999999999999.99m)
            .When(x => x.Price.HasValue)
            .WithMessage("Giá không được vượt quá 999,999,999,999.99");

        // Location validation (optional, but if provided must be valid)
        RuleFor(x => x.Location)
            .NotEmpty()
            .When(x => x.Location != null)
            .WithMessage("Địa điểm không được để trống")
            .MaximumLength(500)
            .When(x => x.Location != null)
            .WithMessage("Địa điểm không được vượt quá 500 ký tự");

        // Brand validation (optional, but if provided must be valid)
        RuleFor(x => x.Brand)
            .NotEmpty()
            .When(x => x.Brand != null)
            .WithMessage("Thương hiệu không được để trống")
            .MaximumLength(100)
            .When(x => x.Brand != null)
            .WithMessage("Thương hiệu không được vượt quá 100 ký tự");

        // Model validation (optional, but if provided must be valid)
        RuleFor(x => x.Model)
            .NotEmpty()
            .When(x => x.Model != null)
            .WithMessage("Model không được để trống")
            .MaximumLength(100)
            .When(x => x.Model != null)
            .WithMessage("Model không được vượt quá 100 ký tự");

        // BatteryCapacityCurrent validation (optional, but if provided must be valid)
        RuleFor(x => x.BatteryCapacityCurrent)
            .GreaterThan(0)
            .When(x => x.BatteryCapacityCurrent.HasValue)
            .WithMessage("Dung lượng pin phải lớn hơn 0")
            .LessThanOrEqualTo(10000)
            .When(x => x.BatteryCapacityCurrent.HasValue)
            .WithMessage("Dung lượng pin không hợp lệ");

        // ChargeCount validation (optional, but if provided must be valid)
        RuleFor(x => x.ChargeCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ChargeCount.HasValue)
            .WithMessage("Số lần sạc phải lớn hơn hoặc bằng 0");

        // ProductionYear validation (optional, but if provided must be valid)
        RuleFor(x => x.ProductionYear)
            .InclusiveBetween(2000, DateTime.Now.Year)
            .When(x => x.ProductionYear.HasValue)
            .WithMessage($"Năm sản xuất phải từ 2000 đến {DateTime.Now.Year}");

        // Condition validation (optional, but if provided must be valid)
        RuleFor(x => x.Condition)
            .NotEmpty()
            .When(x => x.Condition != null)
            .WithMessage("Tình trạng không được để trống")
            .MaximumLength(50)
            .When(x => x.Condition != null)
            .WithMessage("Tình trạng không được vượt quá 50 ký tự");

        // Mileage validation (optional, but if provided must be valid)
        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Mileage.HasValue)
            .WithMessage("Số KM phải lớn hơn hoặc bằng 0")
            .LessThanOrEqualTo(1000000)
            .When(x => x.Mileage.HasValue)
            .WithMessage("Số KM không hợp lệ");

        // NewImages validation (optional, but if provided must be valid)
        RuleFor(x => x.NewImages)
            .Must(images => images == null || images.Count <= 10)
            .When(x => x.NewImages != null)
            .WithMessage("Tối đa 10 ảnh sản phẩm");

        RuleForEach(x => x.NewImages)
            .Must(BeValidImage)
            .When(x => x.NewImages != null)
            .WithMessage("File ảnh không hợp lệ. Chỉ chấp nhận file JPG, PNG, JPEG với kích thước tối đa 5MB");

        // NewProofImage validation (optional, but if provided must be valid)
        RuleFor(x => x.NewProofImage)
            .Must(BeValidImage)
            .When(x => x.NewProofImage != null)
            .WithMessage("File ảnh bằng chứng không hợp lệ. Chỉ chấp nhận file JPG, PNG, JPEG với kích thước tối đa 5MB");

        // Auction validation
        RuleFor(x => x.StartingBid)
            .GreaterThan(0)
            .When(x => x.AuctionEnabled == true)
            .WithMessage("Giá khởi điểm đấu giá phải lớn hơn 0 khi bật chế độ đấu giá");

        RuleFor(x => x.AuctionEndTime)
            .NotNull()
            .When(x => x.AuctionEnabled == true)
            .WithMessage("Thời gian kết thúc đấu giá là bắt buộc khi bật chế độ đấu giá")
            .Must(endTime => endTime.HasValue && endTime.Value > DateTime.UtcNow)
            .When(x => x.AuctionEnabled == true && x.AuctionEndTime.HasValue)
            .WithMessage("Thời gian kết thúc đấu giá phải lớn hơn thời gian hiện tại");

        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.StartingBid ?? 0)
            .When(x => x.AuctionEnabled == true && x.BuyNowPrice.HasValue && x.StartingBid.HasValue)
            .WithMessage("Giá mua ngay phải lớn hơn giá khởi điểm");

        RuleFor(x => x.StartingBid)
            .Null()
            .When(x => x.AuctionEnabled == false)
            .WithMessage("Không được nhập giá khởi điểm khi chưa bật chế độ đấu giá");

        RuleFor(x => x.BuyNowPrice)
            .Null()
            .When(x => x.AuctionEnabled == false)
            .WithMessage("Không được nhập giá mua ngay khi chưa bật chế độ đấu giá");

        RuleFor(x => x.AuctionEndTime)
            .Null()
            .When(x => x.AuctionEnabled == false)
            .WithMessage("Không được nhập thời gian kết thúc khi chưa bật chế độ đấu giá");

        // At least one field must be provided
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("Phải cung cấp ít nhất một trường để cập nhật");
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

    private bool HaveAtLeastOneField(PostUpdateRequest request)
    {
        return request.Title != null ||
               request.Description != null ||
               request.Price.HasValue ||
               request.Location != null ||
               request.Brand != null ||
               request.Model != null ||
               request.BatteryCapacityCurrent.HasValue ||
               request.ChargeCount.HasValue ||
               request.ProductionYear.HasValue ||
               request.Condition != null ||
               request.Mileage.HasValue ||
               (request.NewImages != null && request.NewImages.Any()) ||
               request.NewProofImage != null ||
               (request.ImagesToDelete != null && request.ImagesToDelete.Any()) ||
               request.AuctionEnabled.HasValue ||
               request.StartingBid.HasValue ||
               request.BuyNowPrice.HasValue ||
               request.AuctionEndTime.HasValue;
    }
}

