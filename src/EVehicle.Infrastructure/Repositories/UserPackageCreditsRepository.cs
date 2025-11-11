using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho UserPackageCredits entity
/// </summary>
public class UserPackageCreditsRepository : IUserPackageCreditsRepository
{
    private readonly EVehicleDbContext _context;

    public UserPackageCreditsRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<UserPackageCredits?> GetByUserAndPackageAsync(Guid userId, int packageId)
    {
        return await _context.UserPackageCredits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PackageId == packageId);
    }

    public async Task<List<UserPackageCredits>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPackageCredits
            .AsNoTracking()
            .Include(c => c.Package)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserPackageCredits> CreateAsync(UserPackageCredits userPackageCredits)
    {
        await _context.UserPackageCredits.AddAsync(userPackageCredits);
        return userPackageCredits;
    }

    public async Task<UserPackageCredits> UpdateAsync(UserPackageCredits userPackageCredits)
    {
        _context.UserPackageCredits.Update(userPackageCredits);
        return await Task.FromResult(userPackageCredits);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

