using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho ContractTemplate entity
/// </summary>
public class ContractTemplateRepository : IContractTemplateRepository
{
    private readonly EVehicleDbContext _context;

    public ContractTemplateRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<ContractTemplate?> GetByIdAsync(int templateId)
    {
        return await _context.ContractTemplates
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.TemplateId == templateId);
    }

    public async Task<List<ContractTemplate>> GetAllAsync()
    {
        return await _context.ContractTemplates
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderBy(t => t.TemplateName)
            .ToListAsync();
    }

    public async Task<List<ContractTemplate>> GetByCategoryIdAsync(int? categoryId)
    {
        var query = _context.ContractTemplates
            .AsNoTracking()
            .Include(t => t.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(t => t.TemplateName)
            .ToListAsync();
    }

    public async Task<List<ContractTemplate>> GetActiveTemplatesAsync(int? categoryId = null)
    {
        var query = _context.ContractTemplates
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(t => t.TemplateName)
            .ToListAsync();
    }
}

