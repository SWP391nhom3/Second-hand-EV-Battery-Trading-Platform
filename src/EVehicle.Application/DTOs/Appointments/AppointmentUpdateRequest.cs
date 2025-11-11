namespace EVehicle.Application.DTOs.Appointments;

/// <summary>
/// Request DTO cho việc cập nhật Appointment (UC42)
/// </summary>
public class AppointmentUpdateRequest
{
    /// <summary>
    /// Thời gian bắt đầu
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Trạng thái (CONFIRMED, CANCELED, COMPLETED)
    /// </summary>
    public string? Status { get; set; }
}

