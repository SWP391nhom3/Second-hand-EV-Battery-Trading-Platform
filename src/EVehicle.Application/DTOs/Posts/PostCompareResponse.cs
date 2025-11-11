namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho UC20: So sánh Sản phẩm
/// Chứa thông tin so sánh các sản phẩm
/// </summary>
public class PostCompareResponse
{
    /// <summary>
    /// Danh sách thông tin sản phẩm để so sánh
    /// </summary>
    public List<PostCompareItem> Products { get; set; } = new();
}

/// <summary>
/// Thông tin một sản phẩm trong bảng so sánh
/// </summary>
public class PostCompareItem
{
    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL ảnh đại diện (ảnh đầu tiên)
    /// </summary>
    public string? ThumbnailImageUrl { get; set; }

    /// <summary>
    /// Giá
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Dung lượng pin hiện tại (SOH)
    /// </summary>
    public decimal BatteryCapacityCurrent { get; set; }

    /// <summary>
    /// Số km đã đi (chỉ cho Xe điện)
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Năm sản xuất
    /// </summary>
    public int ProductionYear { get; set; }

    /// <summary>
    /// Hãng (Brand)
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Model
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Tình trạng
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Số lần sạc
    /// </summary>
    public int? ChargeCount { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Có bật chế độ đấu giá
    /// </summary>
    public bool AuctionEnabled { get; set; }

    /// <summary>
    /// Giá khởi điểm đấu giá (nếu có)
    /// </summary>
    public decimal? StartingBid { get; set; }

    /// <summary>
    /// Giá mua ngay (nếu có)
    /// </summary>
    public decimal? BuyNowPrice { get; set; }
}


