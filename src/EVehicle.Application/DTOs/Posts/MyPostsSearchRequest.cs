using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho tìm kiếm bài đăng của user (UC13)
/// </summary>
public class MyPostsSearchRequest : PagedRequest
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
    /// Trạng thái (PENDING, APPROVED, DENIED, DRAFT) - để trống để lấy tất cả
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Sắp xếp theo (createdAt, price, title)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Hướng sắp xếp (asc, desc)
    /// </summary>
    public string? SortDirection { get; set; }
}


