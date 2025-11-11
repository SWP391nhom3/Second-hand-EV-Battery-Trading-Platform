using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Contract entity
/// </summary>
public interface IContractRepository
{
    /// <summary>
    /// Lấy Contract theo ID
    /// </summary>
    Task<Contract?> GetByIdAsync(Guid contractId);

    /// <summary>
    /// Lấy Contract theo ID với đầy đủ thông tin
    /// </summary>
    Task<Contract?> GetByIdWithDetailsAsync(Guid contractId);

    /// <summary>
    /// Lấy Contract theo OrderId
    /// </summary>
    Task<Contract?> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Lấy Contract theo LeadId
    /// </summary>
    Task<Contract?> GetByLeadIdAsync(Guid leadId);

    /// <summary>
    /// Tạo Contract mới
    /// </summary>
    Task<Contract> CreateAsync(Contract contract);

    /// <summary>
    /// Cập nhật Contract
    /// </summary>
    Task<Contract> UpdateAsync(Contract contract);

    /// <summary>
    /// Lấy danh sách Contract theo StaffId
    /// </summary>
    Task<List<Contract>> GetContractsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        Guid? leadId = null,
        Guid? orderId = null);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

