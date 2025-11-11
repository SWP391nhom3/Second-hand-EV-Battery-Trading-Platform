using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO để tạo bài đăng mới (UC06)
/// </summary>
public class PostCreateRequest
{
    /// <summary>
    /// ID danh mục (Xe điện hoặc Pin)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Giá bán
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Model
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Dung lượng pin hiện tại (kWh hoặc Ah)
    /// </summary>
    public decimal BatteryCapacityCurrent { get; set; }

    /// <summary>
    /// Số lần sạc
    /// </summary>
    public int? ChargeCount { get; set; }

    /// <summary>
    /// Năm sản xuất
    /// </summary>
    public int ProductionYear { get; set; }

    /// <summary>
    /// Tình trạng (Mới, Cũ, Đã qua sử dụng)
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Số KM đã đi (chỉ dành cho Xe điện)
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Danh sách ảnh sản phẩm
    /// </summary>
    public List<FileUploadDto> Images { get; set; } = new();

    /// <summary>
    /// Ảnh bằng chứng SOH/KM (bắt buộc)
    /// </summary>
    public FileUploadDto? ProofImage { get; set; }

    /// <summary>
    /// ID gói tin (Basic, Premium, Luxury)
    /// </summary>
    public int PackageId { get; set; }

    /// <summary>
    /// Bật chế độ đấu giá
    /// </summary>
    public bool AuctionEnabled { get; set; } = false;

    /// <summary>
    /// Giá khởi điểm đấu giá (bắt buộc nếu AuctionEnabled = true)
    /// </summary>
    public decimal? StartingBid { get; set; }

    /// <summary>
    /// Giá mua ngay (tùy chọn, nếu đấu giá đạt giá này thì tự động thắng)
    /// </summary>
    public decimal? BuyNowPrice { get; set; }

    /// <summary>
    /// Thời gian kết thúc đấu giá (bắt buộc nếu AuctionEnabled = true)
    /// </summary>
    public DateTime? AuctionEndTime { get; set; }
}

