using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Lead entity
/// </summary>
public class LeadRepository : ILeadRepository
{
    private readonly EVehicleDbContext _context;

    public LeadRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid leadId)
    {
        return await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId);
    }

    public async Task<Lead?> GetByIdWithDetailsAsync(Guid leadId)
    {
        return await _context.Leads
            .AsNoTracking()
            .Include(l => l.Post)
                .ThenInclude(p => p!.User)
            .Include(l => l.Buyer)
            .Include(l => l.Staff)
            .Include(l => l.AssignedByUser)
            .FirstOrDefaultAsync(l => l.Id == leadId);
    }

    /// <summary>
    /// Lấy Lead theo ID với đầy đủ thông tin (có tracking để update)
    /// </summary>
    public async Task<Lead?> GetByIdWithDetailsForUpdateAsync(Guid leadId)
    {
        return await _context.Leads
            .Include(l => l.Post)
            .Include(l => l.Buyer)
            .Include(l => l.Staff)
            .Include(l => l.AssignedByUser)
            .FirstOrDefaultAsync(l => l.Id == leadId);
    }

    public async Task<Lead?> GetByPostIdAndBuyerIdAsync(Guid postId, Guid buyerId)
    {
        return await _context.Leads
            .AsNoTracking()
            .Include(l => l.Post)
            .Include(l => l.Buyer)
            .FirstOrDefaultAsync(l => l.PostId == postId && l.BuyerId == buyerId);
    }

    public async Task<List<Lead>> GetLeadsByPostIdAsync(Guid postId)
    {
        return await _context.Leads
            .AsNoTracking()
            .Include(l => l.Buyer)
            .Include(l => l.Staff)
            .Where(l => l.PostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Lead>> GetLeadsByBuyerIdAsync(Guid buyerId)
    {
        return await _context.Leads
            .AsNoTracking()
            .Include(l => l.Post)
            .Include(l => l.Staff)
            .Where(l => l.BuyerId == buyerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Lead>> GetLeadsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        string? leadType = null,
        Guid? postId = null,
        Guid? buyerId = null)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Include(l => l.Post)
            .Include(l => l.Buyer)
            .Include(l => l.Staff)
            .Include(l => l.AssignedByUser)
            .Where(l => l.StaffId == staffId);

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(l => l.Status == status);
        }

        // Filter by leadType
        if (!string.IsNullOrEmpty(leadType))
        {
            query = query.Where(l => l.LeadType == leadType);
        }

        // Filter by postId
        if (postId.HasValue)
        {
            query = query.Where(l => l.PostId == postId.Value);
        }

        // Filter by buyerId
        if (buyerId.HasValue)
        {
            query = query.Where(l => l.BuyerId == buyerId.Value);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Lead>> GetAllLeadsAsync(
        string? status = null,
        string? leadType = null,
        Guid? postId = null,
        Guid? buyerId = null,
        Guid? staffId = null)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Include(l => l.Post)
            .Include(l => l.Buyer)
            .Include(l => l.Staff)
            .Include(l => l.AssignedByUser)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(l => l.Status == status);
        }

        // Filter by leadType
        if (!string.IsNullOrEmpty(leadType))
        {
            query = query.Where(l => l.LeadType == leadType);
        }

        // Filter by postId
        if (postId.HasValue)
        {
            query = query.Where(l => l.PostId == postId.Value);
        }

        // Filter by buyerId
        if (buyerId.HasValue)
        {
            query = query.Where(l => l.BuyerId == buyerId.Value);
        }

        // Filter by staffId
        if (staffId.HasValue)
        {
            query = query.Where(l => l.StaffId == staffId.Value);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<Lead> CreateAsync(Lead lead)
    {
        await _context.Leads.AddAsync(lead);
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead)
    {
        _context.Leads.Update(lead);
        return await Task.FromResult(lead);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

