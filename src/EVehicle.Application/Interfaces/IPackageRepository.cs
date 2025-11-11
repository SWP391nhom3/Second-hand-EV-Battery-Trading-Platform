using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Package Repository
/// </summary>
public interface IPackageRepository
{
    Task<PackageDefinition?> GetByIdAsync(int packageId);
    Task<PackageDefinition?> GetByIdForUpdateAsync(int packageId);
    Task<List<PackageDefinition>> GetAllActiveAsync();
    Task<bool> ExistsAsync(int packageId);
    Task<bool> ExistsByNameAsync(string name, int? excludePackageId = null);
    Task<PagedResult<PackageDefinition>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        bool? isActive = null);
    Task<PackageDefinition> CreateAsync(PackageDefinition package);
    Task<PackageDefinition> UpdateAsync(PackageDefinition package);
    Task SaveChangesAsync();
}

