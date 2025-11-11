using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO để chỉnh sửa bài đăng (UC07)
/// </summary>
public class PostUpdateRequest
{
    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Mô tả chi tiết
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá bán
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Dung lượng pin hiện tại (kWh hoặc Ah)
    /// </summary>
    public decimal? BatteryCapacityCurrent { get; set; }

    /// <summary>
    /// Số lần sạc
    /// </summary>
    public int? ChargeCount { get; set; }

    /// <summary>
    /// Năm sản xuất
    /// </summary>
    public int? ProductionYear { get; set; }

    /// <summary>
    /// Tình trạng (Mới, Cũ, Đã qua sử dụng)
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Số KM đã đi (chỉ dành cho Xe điện)
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Danh sách ảnh sản phẩm mới (nếu muốn thay thế)
    /// </summary>
    public List<FileUploadDto>? NewImages { get; set; }

    /// <summary>
    /// Ảnh bằng chứng SOH/KM mới (nếu muốn thay thế)
    /// </summary>
    public FileUploadDto? NewProofImage { get; set; }

    /// <summary>
    /// Danh sách URL ảnh cũ cần xóa (nếu có)
    /// </summary>
    public List<string>? ImagesToDelete { get; set; }

    /// <summary>
    /// Đấu giá
    /// </summary>
    public bool? AuctionEnabled { get; set; }

    /// <summary>
    /// Giá khởi điểm đấu giá
    /// </summary>
    public decimal? StartingBid { get; set; }

    /// <summary>
    /// Giá mua ngay (nếu có)
    /// </summary>
    public decimal? BuyNowPrice { get; set; }

    /// <summary>
    /// Thời gian kết thúc đấu giá
    /// </summary>
    public DateTime? AuctionEndTime { get; set; }
}

