using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Lead entity
/// </summary>
public interface ILeadRepository
{
    /// <summary>
    /// Lấy Lead theo ID
    /// </summary>
    Task<Lead?> GetByIdAsync(Guid leadId);

    /// <summary>
    /// Lấy Lead theo ID với đầy đủ thông tin
    /// </summary>
    Task<Lead?> GetByIdWithDetailsAsync(Guid leadId);

    /// <summary>
    /// Lấy Lead theo ID với đầy đủ thông tin (có tracking để update)
    /// </summary>
    Task<Lead?> GetByIdWithDetailsForUpdateAsync(Guid leadId);

    /// <summary>
    /// Lấy Lead theo PostId và BuyerId
    /// </summary>
    Task<Lead?> GetByPostIdAndBuyerIdAsync(Guid postId, Guid buyerId);

    /// <summary>
    /// Lấy tất cả Leads của một Post
    /// </summary>
    Task<List<Lead>> GetLeadsByPostIdAsync(Guid postId);

    /// <summary>
    /// Lấy tất cả Leads của một Buyer
    /// </summary>
    Task<List<Lead>> GetLeadsByBuyerIdAsync(Guid buyerId);

    /// <summary>
    /// Lấy tất cả Leads của một Staff (UC40)
    /// </summary>
    Task<List<Lead>> GetLeadsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        string? leadType = null,
        Guid? postId = null,
        Guid? buyerId = null);

    /// <summary>
    /// UC46: Lấy tất cả Leads (dành cho Admin)
    /// </summary>
    Task<List<Lead>> GetAllLeadsAsync(
        string? status = null,
        string? leadType = null,
        Guid? postId = null,
        Guid? buyerId = null,
        Guid? staffId = null);

    /// <summary>
    /// Tạo Lead mới
    /// </summary>
    Task<Lead> CreateAsync(Lead lead);

    /// <summary>
    /// Cập nhật Lead
    /// </summary>
    Task<Lead> UpdateAsync(Lead lead);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

