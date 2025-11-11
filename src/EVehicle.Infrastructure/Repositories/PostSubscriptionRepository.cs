using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho PostSubscription entity
/// </summary>
public class PostSubscriptionRepository : IPostSubscriptionRepository
{
    private readonly EVehicleDbContext _context;

    public PostSubscriptionRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<PostSubscription?> GetByPostIdAsync(Guid postId)
    {
        return await _context.PostSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.PostId == postId);
    }

    public async Task<PostSubscription> CreateAsync(PostSubscription subscription)
    {
        await _context.PostSubscriptions.AddAsync(subscription);
        return subscription;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

