using EVehicle.Application.DTOs.Appointments;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Appointment operations
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// UC41: Tạo Appointment (Tạo Lịch hẹn)
    /// </summary>
    Task<BaseResponse<AppointmentResponse>> CreateAppointmentAsync(
        Guid staffId,
        AppointmentCreateRequest request);

    /// <summary>
    /// UC42: Lấy danh sách Appointments (Quản lý Lịch hẹn)
    /// </summary>
    Task<BaseResponse<PagedResponse<AppointmentResponse>>> GetAppointmentsAsync(
        Guid staffId,
        AppointmentSearchRequest request);

    /// <summary>
    /// UC42: Lấy chi tiết Appointment
    /// </summary>
    Task<BaseResponse<AppointmentResponse>> GetAppointmentByIdAsync(
        Guid appointmentId,
        Guid staffId);

    /// <summary>
    /// UC42: Cập nhật Appointment
    /// </summary>
    Task<BaseResponse<AppointmentResponse>> UpdateAppointmentAsync(
        Guid appointmentId,
        Guid staffId,
        AppointmentUpdateRequest request);

    /// <summary>
    /// UC42: Hủy Appointment
    /// </summary>
    Task<BaseResponse<AppointmentResponse>> CancelAppointmentAsync(
        Guid appointmentId,
        Guid staffId);

    /// <summary>
    /// UC42: Cập nhật trạng thái Appointment
    /// </summary>
    Task<BaseResponse<AppointmentResponse>> UpdateAppointmentStatusAsync(
        Guid appointmentId,
        Guid staffId,
        AppointmentStatusUpdateRequest request);
}

