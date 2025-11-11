using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Leads;

/// <summary>
/// Request DTO cho việc tìm kiếm Lead (UC40)
/// </summary>
public class LeadSearchRequest : PagedRequest
{
    /// <summary>
    /// Lọc theo StaffId (mặc định là Staff hiện tại)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Lọc theo trạng thái (NEW, ASSIGNED, CONTACTED, SCHEDULED, SUCCESSFUL, FAILED)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Lọc theo loại Lead (SCHEDULE_VIEW, AUCTION_WINNER)
    /// </summary>
    public string? LeadType { get; set; }

    /// <summary>
    /// Lọc theo PostId
    /// </summary>
    public Guid? PostId { get; set; }

    /// <summary>
    /// Lọc theo BuyerId
    /// </summary>
    public Guid? BuyerId { get; set; }

    /// <summary>
    /// Sắp xếp theo (CreatedAt, AssignedAt, Status)
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Thứ tự sắp xếp (ASC, DESC)
    /// </summary>
    public string? SortOrder { get; set; } = "DESC";
}

