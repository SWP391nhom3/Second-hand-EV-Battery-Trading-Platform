using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Favorite entity
/// </summary>
public class FavoriteRepository : IFavoriteRepository
{
    private readonly EVehicleDbContext _context;

    public FavoriteRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Favorite?> GetByIdAsync(Guid favoriteId)
    {
        return await _context.Favorites
            .AsNoTracking()
            .Include(f => f.Post)
                .ThenInclude(p => p!.PostImages)
            .Include(f => f.Post)
                .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(f => f.Id == favoriteId);
    }

    public async Task<Favorite?> GetByUserAndPostAsync(Guid userId, Guid postId)
    {
        return await _context.Favorites
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PostId == postId);
    }

    public async Task<Favorite?> GetByUserAndPostForDeleteAsync(Guid userId, Guid postId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PostId == postId);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid postId)
    {
        return await _context.Favorites
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.PostId == postId);
    }

    public async Task<PagedResult<Favorite>> GetFavoritesByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? status,
        bool? isActive,
        bool? isSold,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Favorites
            .AsNoTracking()
            .Include(f => f.Post)
                .ThenInclude(p => p!.PostImages)
            .Include(f => f.Post)
                .ThenInclude(p => p!.Category)
            .Where(f => f.UserId == userId);

        // Apply filters on Post
        query = query.Where(f => f.Post != null);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(f =>
                f.Post!.Title.Contains(keyword) ||
                (f.Post!.Description != null && f.Post.Description.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.Post!.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(f => f.Post!.Status == status);
        }

        if (isActive.HasValue)
        {
            query = query.Where(f => f.Post!.IsActive == isActive.Value);
        }

        if (isSold.HasValue)
        {
            query = query.Where(f => f.Post!.IsSold == isSold.Value);
        }

        // Apply sorting
        sortBy = string.IsNullOrWhiteSpace(sortBy) ? "created_at" : sortBy.ToLower();
        sortDirection = string.IsNullOrWhiteSpace(sortDirection) ? "desc" : sortDirection.ToLower();

        query = sortBy switch
        {
            "created_at" => sortDirection == "asc"
                ? query.OrderBy(f => f.CreatedAt)
                : query.OrderByDescending(f => f.CreatedAt),
            "price" => sortDirection == "asc"
                ? query.OrderBy(f => f.Post!.Price)
                : query.OrderByDescending(f => f.Post!.Price),
            "title" => sortDirection == "asc"
                ? query.OrderBy(f => f.Post!.Title)
                : query.OrderByDescending(f => f.Post!.Title),
            _ => query.OrderByDescending(f => f.CreatedAt)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var favorites = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Favorite>.Create(favorites, totalCount, pageNumber, pageSize);
    }

    public async Task<Favorite> CreateAsync(Favorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.CreatedAt = DateTime.UtcNow;
        
        await _context.Favorites.AddAsync(favorite);
        return favorite;
    }

    public async Task DeleteAsync(Favorite favorite)
    {
        _context.Favorites.Remove(favorite);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

