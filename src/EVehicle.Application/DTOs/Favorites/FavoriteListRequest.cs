using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Favorites;

/// <summary>
/// Request DTO cho danh sách yêu thích
/// </summary>
public class FavoriteListRequest : PagedRequest
{
    /// <summary>
    /// Từ khóa tìm kiếm (tìm trong tiêu đề, mô tả)
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// ID danh mục (lọc theo category)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Lọc theo trạng thái bài đăng (APPROVED, DENIED, etc.)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Lọc chỉ bài đăng đang hoạt động
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Lọc chỉ bài đăng chưa bán
    /// </summary>
    public bool? IsSold { get; set; }
}

