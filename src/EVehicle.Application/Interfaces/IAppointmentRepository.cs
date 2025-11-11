using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Appointment entity
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Lấy Appointment theo ID
    /// </summary>
    Task<Appointment?> GetByIdAsync(Guid appointmentId);

    /// <summary>
    /// Lấy Appointment theo ID với đầy đủ thông tin
    /// </summary>
    Task<Appointment?> GetByIdWithDetailsAsync(Guid appointmentId);

    /// <summary>
    /// Lấy tất cả Appointments của một Staff
    /// </summary>
    Task<List<Appointment>> GetAppointmentsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        bool? upcoming = null,
        bool? past = null,
        Guid? leadId = null,
        Guid? postId = null);

    /// <summary>
    /// Lấy tất cả Appointments của một Lead
    /// </summary>
    Task<List<Appointment>> GetAppointmentsByLeadIdAsync(Guid leadId);

    /// <summary>
    /// Tạo Appointment mới
    /// </summary>
    Task<Appointment> CreateAsync(Appointment appointment);

    /// <summary>
    /// Cập nhật Appointment
    /// </summary>
    Task<Appointment> UpdateAsync(Appointment appointment);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

