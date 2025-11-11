namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho chi tiết bài đăng (Public view - ẩn thông tin nhạy cảm)
/// </summary>
public class PostPublicDetailResponse
{
    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// ID người bán (không hiển thị thông tin chi tiết)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Model
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Dung lượng pin hiện tại
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
    /// Tình trạng
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Số KM đã đi (chỉ cho Xe điện)
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Trạng thái (APPROVED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Đang hoạt động
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Đã bán
    /// </summary>
    public bool IsSold { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Danh sách URL ảnh
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// URL ảnh bằng chứng SOH/KM
    /// </summary>
    public string? ProofImageUrl { get; set; }

    /// <summary>
    /// Có bật chế độ đấu giá
    /// </summary>
    public bool AuctionEnabled { get; set; }

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

    /// <summary>
    /// Tên người bán (chỉ hiển thị tên, không hiển thị email/phone)
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Mức độ ưu tiên của gói tin (3=Luxury, 2=Premium, 1=Basic)
    /// Càng cao thì bài đăng càng được hiển thị lên top
    /// </summary>
    public int? PriorityLevel { get; set; }
}

