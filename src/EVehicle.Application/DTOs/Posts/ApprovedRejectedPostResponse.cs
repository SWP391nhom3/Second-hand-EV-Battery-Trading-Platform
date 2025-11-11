namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho danh sách bài đăng đã duyệt hoặc từ chối
/// </summary>
public class ApprovedRejectedPostResponse
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
    /// Trạng thái (APPROVED, DENIED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian duyệt (nếu đã duyệt)
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Thời gian từ chối (nếu đã từ chối)
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// Lý do từ chối (nếu đã từ chối)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Tên Staff được gán (nếu đã duyệt)
    /// </summary>
    public string? AssignedStaffName { get; set; }

    /// <summary>
    /// Tên Admin đã duyệt (nếu đã duyệt)
    /// </summary>
    public string? ApprovedByName { get; set; }

    /// <summary>
    /// Email Admin đã duyệt (nếu đã duyệt)
    /// </summary>
    public string? ApprovedByEmail { get; set; }

    /// <summary>
    /// Tên Admin đã từ chối (nếu đã từ chối)
    /// </summary>
    public string? RejectedByName { get; set; }

    /// <summary>
    /// Email Admin đã từ chối (nếu đã từ chối)
    /// </summary>
    public string? RejectedByEmail { get; set; }

    /// <summary>
    /// URL ảnh thumbnail
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Có ảnh bằng chứng SOH/KM không
    /// </summary>
    public bool HasProofImage { get; set; }
}

