using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho UserPackageCredits Repository
/// </summary>
public interface IUserPackageCreditsRepository
{
    Task<UserPackageCredits?> GetByUserAndPackageAsync(Guid userId, int packageId);
    Task<List<UserPackageCredits>> GetByUserIdAsync(Guid userId);
    Task<UserPackageCredits> CreateAsync(UserPackageCredits userPackageCredits);
    Task<UserPackageCredits> UpdateAsync(UserPackageCredits userPackageCredits);
    Task SaveChangesAsync();
}

