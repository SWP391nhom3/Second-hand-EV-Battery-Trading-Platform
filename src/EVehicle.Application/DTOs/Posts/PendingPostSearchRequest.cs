using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho tìm kiếm bài đăng chờ duyệt
/// </summary>
public class PendingPostSearchRequest : PagedRequest
{
    /// <summary>
    /// Từ khóa tìm kiếm (tiêu đề, mô tả)
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
    /// Sắp xếp theo (createdAt, price, title)
    /// </summary>
    public string? SortBy { get; set; }
}

