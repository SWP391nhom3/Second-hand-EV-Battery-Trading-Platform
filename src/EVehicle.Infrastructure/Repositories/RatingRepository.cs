using EVehicle.Application.Interfaces;
using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Rating entity
/// </summary>
public class RatingRepository : IRatingRepository
{
    private readonly EVehicleDbContext _context;

    public RatingRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetByIdAsync(Guid ratingId)
    {
        return await _context.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ratingId);
    }

    public async Task<Rating?> GetByIdForUpdateAsync(Guid ratingId)
    {
        return await _context.Ratings
            .FirstOrDefaultAsync(r => r.Id == ratingId);
    }

    public async Task<Rating?> GetByIdWithDetailsAsync(Guid ratingId)
    {
        return await _context.Ratings
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.Rater)
            .Include(r => r.Ratee)
            .Include(r => r.RatingReplies)
                .ThenInclude(rr => rr.User)
            .FirstOrDefaultAsync(r => r.Id == ratingId);
    }

    public async Task<Rating?> GetByOrderAndUsersAsync(Guid orderId, Guid raterId, Guid rateeId)
    {
        return await _context.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => 
                r.OrderId == orderId && 
                r.RaterId == raterId && 
                r.RateeId == rateeId);
    }

    public async Task<PagedResult<Rating>> GetRatingsByRateeIdAsync(
        Guid rateeId,
        string? rateeRole,
        int? minScore,
        int? maxScore,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Ratings
            .AsNoTracking()
            .Include(r => r.Rater)
            .Include(r => r.Order)
            .Include(r => r.RatingReplies)
            .Where(r => r.RateeId == rateeId);

        // Filter by role
        if (!string.IsNullOrWhiteSpace(rateeRole))
        {
            query = query.Where(r => r.RateeRole == rateeRole);
        }

        // Filter by score range
        if (minScore.HasValue)
        {
            query = query.Where(r => r.Score >= minScore.Value);
        }

        if (maxScore.HasValue)
        {
            query = query.Where(r => r.Score <= maxScore.Value);
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
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Rating>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Rating>> GetRatingsByOrderIdAsync(Guid orderId)
    {
        return await _context.Ratings
            .AsNoTracking()
            .Include(r => r.Rater)
            .Include(r => r.Ratee)
            .Include(r => r.RatingReplies)
                .ThenInclude(rr => rr.User)
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid orderId, Guid raterId, Guid rateeId)
    {
        return await _context.Ratings
            .AnyAsync(r => 
                r.OrderId == orderId && 
                r.RaterId == raterId && 
                r.RateeId == rateeId);
    }

    public async Task<Rating> CreateAsync(Rating rating)
    {
        await _context.Ratings.AddAsync(rating);
        return rating;
    }

    public async Task<Rating> UpdateAsync(Rating rating)
    {
        _context.Ratings.Update(rating);
        return await Task.FromResult(rating);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private static System.Linq.Expressions.Expression<Func<Rating, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "score" => r => r.Score,
            "createdat" => r => r.CreatedAt,
            "updatedat" => r => r.UpdatedAt ?? r.CreatedAt,
            _ => r => r.CreatedAt
        };
    }
}

