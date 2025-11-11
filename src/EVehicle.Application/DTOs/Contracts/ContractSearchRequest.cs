using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Contracts;

/// <summary>
/// Request DTO cho việc tìm kiếm Contract (UC43)
/// </summary>
public class ContractSearchRequest : PagedRequest
{
    /// <summary>
    /// Lọc theo StaffId (mặc định là Staff hiện tại)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Lọc theo trạng thái (DRAFT, PENDING_SIGNATURE, SIGNED)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Lọc theo LeadId
    /// </summary>
    public Guid? LeadId { get; set; }

    /// <summary>
    /// Lọc theo OrderId
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Sắp xếp theo (CreatedAt, Status, SignedAt)
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Thứ tự sắp xếp (ASC, DESC)
    /// </summary>
    public string? SortOrder { get; set; } = "DESC";
}

