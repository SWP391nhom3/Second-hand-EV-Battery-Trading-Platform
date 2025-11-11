using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Bid entity
/// </summary>
public class BidRepository : IBidRepository
{
    private readonly EVehicleDbContext _context;

    public BidRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Bid?> GetByIdAsync(Guid bidId)
    {
        return await _context.Bids
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Post)
            .FirstOrDefaultAsync(b => b.Id == bidId);
    }

    public async Task<List<Bid>> GetBidsByPostIdAsync(Guid postId)
    {
        return await _context.Bids
            .AsNoTracking()
            .Include(b => b.User)
            .Where(b => b.PostId == postId)
            .OrderByDescending(b => b.BidAmount)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Bid?> GetHighestBidByPostIdAsync(Guid postId)
    {
        return await _context.Bids
            .AsNoTracking()
            .Include(b => b.User)
            .Where(b => b.PostId == postId)
            .OrderByDescending(b => b.BidAmount)
            .ThenByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetBidCountByPostIdAsync(Guid postId)
    {
        return await _context.Bids
            .CountAsync(b => b.PostId == postId);
    }

    public async Task<Bid> CreateAsync(Bid bid)
    {
        await _context.Bids.AddAsync(bid);
        return bid;
    }

    public async Task<Bid> UpdateAsync(Bid bid)
    {
        _context.Bids.Update(bid);
        return await Task.FromResult(bid);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

