using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Category entity
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly EVehicleDbContext _context;

    public CategoryRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
    }
}

