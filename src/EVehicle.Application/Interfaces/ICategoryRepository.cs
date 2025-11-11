using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Category Repository
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int categoryId);
    Task<List<Category>> GetAllAsync();
    Task<bool> ExistsAsync(int categoryId);
}

