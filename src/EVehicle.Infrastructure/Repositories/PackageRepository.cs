using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho PackageDefinition entity
/// </summary>
public class PackageRepository : IPackageRepository
{
    private readonly EVehicleDbContext _context;

    public PackageRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<PackageDefinition?> GetByIdAsync(int packageId)
    {
        return await _context.PackageDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PackageId == packageId);
    }

    public async Task<PackageDefinition?> GetByIdForUpdateAsync(int packageId)
    {
        return await _context.PackageDefinitions
            .FirstOrDefaultAsync(p => p.PackageId == packageId);
    }

    public async Task<List<PackageDefinition>> GetAllActiveAsync()
    {
        return await _context.PackageDefinitions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriorityLevel)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int packageId)
    {
        return await _context.PackageDefinitions.AnyAsync(p => p.PackageId == packageId);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludePackageId = null)
    {
        var query = _context.PackageDefinitions
            .Where(p => p.Name.ToLower() == name.ToLower());
        
        if (excludePackageId.HasValue)
        {
            query = query.Where(p => p.PackageId != excludePackageId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<PagedResult<PackageDefinition>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        bool? isActive = null)
    {
        var query = _context.PackageDefinitions.AsNoTracking();

        // Filter by IsActive
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<PackageDefinition>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PackageDefinition> CreateAsync(PackageDefinition package)
    {
        await _context.PackageDefinitions.AddAsync(package);
        return package;
    }

    public async Task<PackageDefinition> UpdateAsync(PackageDefinition package)
    {
        _context.PackageDefinitions.Update(package);
        return await Task.FromResult(package);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private static System.Linq.Expressions.Expression<Func<PackageDefinition, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "name" => p => p.Name,
            "price" => p => p.Price,
            "creditscount" => p => p.CreditsCount,
            "prioritylevel" => p => p.PriorityLevel,
            "maximages" => p => p.MaxImages,
            "isactive" => p => p.IsActive,
            "createdat" => p => p.CreatedAt,
            _ => p => p.CreatedAt
        };
    }
}

