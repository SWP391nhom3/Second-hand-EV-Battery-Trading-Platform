using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho User Repository
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetBySocialLoginAsync(string provider, string socialLoginId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task SaveChangesAsync();
    Task<List<User>> GetAdminsAsync();
    Task<List<User>> GetStaffAsync();
    
    /// <summary>
    /// UC47: Lấy danh sách người dùng với phân trang và lọc
    /// </summary>
    Task<PagedResult<User>> GetUsersAsync(
        string? keyword,
        string? role,
        string? status,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection);
}

