namespace EVehicle.Application.DTOs.Appointments;

/// <summary>
/// Request DTO cho việc cập nhật trạng thái Appointment
/// </summary>
public class AppointmentStatusUpdateRequest
{
    /// <summary>
    /// Trạng thái mới (CONFIRMED, CANCELED, COMPLETED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Ghi chú (optional) - Lý do thay đổi trạng thái
    /// </summary>
    public string? Notes { get; set; }
}

