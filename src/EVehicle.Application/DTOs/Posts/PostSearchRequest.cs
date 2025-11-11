using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho tìm kiếm bài đăng (cho Member/Public)
/// </summary>
public class PostSearchRequest : PagedRequest
{
    /// <summary>
    /// Từ khóa tìm kiếm (tìm trong title, description)
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Giá tối thiểu
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Giá tối đa
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Năm sản xuất từ
    /// </summary>
    public int? MinProductionYear { get; set; }

    /// <summary>
    /// Năm sản xuất đến
    /// </summary>
    public int? MaxProductionYear { get; set; }

    /// <summary>
    /// Dung lượng pin hiện tại tối thiểu (SOH - State of Health, đơn vị: kWh hoặc Ah)
    /// </summary>
    public decimal? MinBatteryCapacity { get; set; }

    /// <summary>
    /// Dung lượng pin hiện tại tối đa (SOH - State of Health, đơn vị: kWh hoặc Ah)
    /// </summary>
    public decimal? MaxBatteryCapacity { get; set; }

    /// <summary>
    /// Số km tối thiểu (chỉ cho Xe điện)
    /// </summary>
    public int? MinMileage { get; set; }

    /// <summary>
    /// Số km tối đa (chỉ cho Xe điện)
    /// </summary>
    public int? MaxMileage { get; set; }

    /// <summary>
    /// Tình trạng
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Chỉ hiển thị bài đăng đang đấu giá
    /// </summary>
    public bool? AuctionOnly { get; set; }
}

