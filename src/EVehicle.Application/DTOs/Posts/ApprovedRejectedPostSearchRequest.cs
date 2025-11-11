using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho tìm kiếm bài đăng đã duyệt hoặc từ chối
/// </summary>
public class ApprovedRejectedPostSearchRequest : PagedRequest
{
    /// <summary>
    /// Trạng thái bài đăng (APPROVED, DENIED). Nếu null thì lấy cả hai
    /// </summary>
    public string? Status { get; set; }

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
    /// Sắp xếp theo (createdAt, approvedAt, rejectedAt, price, title)
    /// </summary>
    public string? SortBy { get; set; }
}


