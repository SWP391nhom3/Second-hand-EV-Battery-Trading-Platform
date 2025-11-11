using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Appointments;

/// <summary>
/// Request DTO cho việc tìm kiếm Appointment (UC42)
/// </summary>
public class AppointmentSearchRequest : PagedRequest
{
    /// <summary>
    /// Lọc theo StaffId (mặc định là Staff hiện tại)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Lọc theo trạng thái (CONFIRMED, CANCELED, COMPLETED)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Lọc theo LeadId
    /// </summary>
    public Guid? LeadId { get; set; }

    /// <summary>
    /// Lọc theo PostId
    /// </summary>
    public Guid? PostId { get; set; }

    /// <summary>
    /// Lọc lịch hẹn sắp tới (StartTime >= now)
    /// </summary>
    public bool? Upcoming { get; set; }

    /// <summary>
    /// Lọc lịch hẹn đã qua (StartTime < now)
    /// </summary>
    public bool? Past { get; set; }

    /// <summary>
    /// Sắp xếp theo (StartTime, CreatedAt)
    /// </summary>
    public string? SortBy { get; set; } = "StartTime";

    /// <summary>
    /// Thứ tự sắp xếp (ASC, DESC)
    /// </summary>
    public string? SortOrder { get; set; } = "ASC";
}

