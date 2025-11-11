namespace EVehicle.Application.DTOs.Favorites;

/// <summary>
/// Response DTO cho bài đăng yêu thích
/// </summary>
public class FavoriteResponse
{
    /// <summary>
    /// ID yêu thích
    /// </summary>
    public Guid FavoriteId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// ID người dùng
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Thời gian thêm vào yêu thích
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thông tin bài đăng
    /// </summary>
    public PostInfo? Post { get; set; }
}

/// <summary>
/// Thông tin cơ bản của bài đăng trong danh sách yêu thích
/// </summary>
public class PostInfo
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
    /// Trạng thái (APPROVED, DENIED, etc.)
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
    /// URL ảnh thumbnail (ảnh đầu tiên)
    /// </summary>
    public string? ThumbnailImageUrl { get; set; }

    /// <summary>
    /// Thời gian tạo bài đăng
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

