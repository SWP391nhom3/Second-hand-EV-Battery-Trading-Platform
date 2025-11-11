using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Leads;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Lead operations
/// </summary>
public interface ILeadService
{
    /// <summary>
    /// UC23: Tạo Lead (Đặt lịch xem / Yêu cầu Môi giới)
    /// </summary>
    Task<BaseResponse<LeadResponse>> CreateLeadAsync(
        Guid userId,
        LeadCreateRequest request);

    /// <summary>
    /// UC40: Lấy danh sách Lead được gán cho Staff
    /// </summary>
    Task<BaseResponse<PagedResponse<LeadResponse>>> GetLeadsByStaffIdAsync(
        Guid staffId,
        LeadSearchRequest request);

    /// <summary>
    /// Lấy chi tiết Lead
    /// </summary>
    Task<BaseResponse<LeadResponse>> GetLeadByIdAsync(
        Guid leadId,
        Guid? staffId = null);

    /// <summary>
    /// UC44: Cập nhật trạng thái Lead
    /// </summary>
    Task<BaseResponse<LeadResponse>> UpdateLeadStatusAsync(
        Guid staffId,
        Guid leadId,
        LeadStatusUpdateRequest request);

    /// <summary>
    /// UC46: Admin gán Staff cho Lead
    /// </summary>
    Task<BaseResponse<LeadResponse>> AssignStaffToLeadAsync(
        Guid adminId,
        Guid leadId,
        LeadAssignStaffRequest request);

    /// <summary>
    /// UC46: Lấy danh sách tất cả Leads (dành cho Admin)
    /// </summary>
    Task<PagedResponse<LeadResponse>> GetAllLeadsAsync(
        LeadSearchRequest request);

    /// <summary>
    /// UC23: Lấy danh sách Leads của Member (người mua)
    /// </summary>
    Task<BaseResponse<PagedResponse<LeadResponse>>> GetMyLeadsAsync(
        Guid buyerId,
        LeadSearchRequest request);
}

