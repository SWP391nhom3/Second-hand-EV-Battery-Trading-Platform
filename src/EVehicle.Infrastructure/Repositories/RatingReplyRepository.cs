using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho RatingReply entity
/// </summary>
public class RatingReplyRepository : IRatingReplyRepository
{
    private readonly EVehicleDbContext _context;

    public RatingReplyRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<RatingReply?> GetByIdAsync(Guid replyId)
    {
        return await _context.RatingReplies
            .AsNoTracking()
            .Include(rr => rr.User)
            .Include(rr => rr.Rating)
            .FirstOrDefaultAsync(rr => rr.Id == replyId);
    }

    public async Task<List<RatingReply>> GetRepliesByRatingIdAsync(Guid ratingId)
    {
        return await _context.RatingReplies
            .AsNoTracking()
            .Include(rr => rr.User)
            .Where(rr => rr.RatingId == ratingId)
            .OrderBy(rr => rr.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UserHasRepliedAsync(Guid ratingId, Guid userId)
    {
        return await _context.RatingReplies
            .AnyAsync(rr => rr.RatingId == ratingId && rr.UserId == userId);
    }

    public async Task<RatingReply> CreateAsync(RatingReply reply)
    {
        await _context.RatingReplies.AddAsync(reply);
        return reply;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


