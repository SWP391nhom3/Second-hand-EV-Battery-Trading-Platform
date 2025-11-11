namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho danh sách bài đăng chờ duyệt
/// </summary>
public class PendingPostResponse
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
    /// Giá
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Model
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Email người bán
    /// </summary>
    public string SellerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// URL ảnh thumbnail
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Có ảnh bằng chứng SOH/KM không
    /// </summary>
    public bool HasProofImage { get; set; }
}

