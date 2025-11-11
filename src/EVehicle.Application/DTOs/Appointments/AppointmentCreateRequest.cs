namespace EVehicle.Application.DTOs.Appointments;

/// <summary>
/// Request DTO cho việc tạo Appointment (UC41)
/// </summary>
public class AppointmentCreateRequest
{
    /// <summary>
    /// ID Lead
    /// </summary>
    public Guid LeadId { get; set; }

    /// <summary>
    /// Thời gian bắt đầu
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc (optional)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
}

