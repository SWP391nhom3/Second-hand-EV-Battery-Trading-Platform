namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho chi tiết bài đăng (bao gồm thông tin Staff và Subscription)
/// </summary>
public class PostDetailResponse : PostResponse
{
    /// <summary>
    /// Lý do từ chối (nếu status = DENIED)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Thời gian được duyệt
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Thông tin Admin đã duyệt
    /// </summary>
    public AdminInfo? ApprovedBy { get; set; }

    /// <summary>
    /// Thời gian bị từ chối
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// Thông tin Admin đã từ chối
    /// </summary>
    public AdminInfo? RejectedBy { get; set; }

    /// <summary>
    /// Thông tin Staff được gán
    /// </summary>
    public StaffAssignmentInfo? StaffAssignment { get; set; }

    /// <summary>
    /// Thông tin gói tin đã sử dụng
    /// </summary>
    public SubscriptionInfo? Subscription { get; set; }

    /// <summary>
    /// Thông tin người bán
    /// </summary>
    public SellerInfo? Seller { get; set; }
}

/// <summary>
/// Thông tin Staff được gán
/// </summary>
public class StaffAssignmentInfo
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

/// <summary>
/// Thông tin gói tin đã sử dụng
/// </summary>
public class SubscriptionInfo
{
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
    public DateTime AppliedAt { get; set; }
}

/// <summary>
/// Thông tin người bán
/// </summary>
public class SellerInfo
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// Thông tin Admin
/// </summary>
public class AdminInfo
{
    public Guid AdminId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

