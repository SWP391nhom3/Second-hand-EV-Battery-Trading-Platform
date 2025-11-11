using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Contract entity
/// </summary>
public class ContractRepository : IContractRepository
{
    private readonly EVehicleDbContext _context;

    public ContractRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(Guid contractId)
    {
        return await _context.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contractId);
    }

    public async Task<Contract?> GetByIdWithDetailsAsync(Guid contractId)
    {
        return await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Order)
                .ThenInclude(o => o!.Buyer)
            .Include(c => c.Order)
                .ThenInclude(o => o!.Seller)
            .Include(c => c.Order)
                .ThenInclude(o => o!.Post)
            .Include(c => c.Lead)
                .ThenInclude(l => l!.Buyer)
            .Include(c => c.Lead)
                .ThenInclude(l => l!.Post)
                    .ThenInclude(p => p!.User)
            .Include(c => c.ContractTemplate)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == contractId);
    }

    public async Task<Contract?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Contracts
            .AsNoTracking()
            .Include(c => c.ContractTemplate)
            .Include(c => c.Order)
            .FirstOrDefaultAsync(c => c.OrderId == orderId);
    }

    public async Task<Contract?> GetByLeadIdAsync(Guid leadId)
    {
        return await _context.Contracts
            .AsNoTracking()
            .Include(c => c.ContractTemplate)
            .Include(c => c.Lead)
            .FirstOrDefaultAsync(c => c.LeadId == leadId);
    }

    public async Task<Contract> CreateAsync(Contract contract)
    {
        await _context.Contracts.AddAsync(contract);
        return contract;
    }

    public async Task<Contract> UpdateAsync(Contract contract)
    {
        _context.Contracts.Update(contract);
        return await Task.FromResult(contract);
    }

    public async Task<List<Contract>> GetContractsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        Guid? leadId = null,
        Guid? orderId = null)
    {
        var query = _context.Contracts
            .AsNoTracking()
            .Include(c => c.ContractTemplate)
            .Include(c => c.CreatedByUser)
            .Include(c => c.Order)
                .ThenInclude(o => o!.Buyer)
            .Include(c => c.Order)
                .ThenInclude(o => o!.Seller)
            .Include(c => c.Lead)
                .ThenInclude(l => l!.Buyer)
            .Include(c => c.Lead)
                .ThenInclude(l => l!.Post)
                    .ThenInclude(p => p!.User)
            .Where(c => c.CreatedBy == staffId);

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        // Filter by leadId
        if (leadId.HasValue)
        {
            query = query.Where(c => c.LeadId == leadId.Value);
        }

        // Filter by orderId
        if (orderId.HasValue)
        {
            query = query.Where(c => c.OrderId == orderId.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

