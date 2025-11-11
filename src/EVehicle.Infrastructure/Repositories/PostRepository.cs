using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Post entity
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly EVehicleDbContext _context;

    public PostRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid postId)
    {
        return await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post?> GetByIdForUpdateAsync(Guid postId)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post?> GetByIdForUpdateWithImagesAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post?> GetByIdWithImagesAsync(Guid postId)
    {
        return await _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.PostSubscriptions)
                .ThenInclude(ps => ps.Package)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post?> GetByIdWithDetailsAsync(Guid postId)
    {
        return await _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.User)
            .Include(p => p.PostStaffAssignments)
                .ThenInclude(psa => psa.Staff)
            .Include(p => p.PostStaffAssignments)
                .ThenInclude(psa => psa.AssignedByUser)
            .Include(p => p.PostSubscriptions)
                .ThenInclude(ps => ps.Package)
            .Include(p => p.PostSubscriptions)
                .ThenInclude(ps => ps.UserPackageCredits)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<PagedResult<Post>> GetPendingPostsAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? brand,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.User)
            .Where(p => p.Status == "PENDING");

        // Apply filters
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Title.Contains(keyword) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
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

        return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<Post>> GetApprovedRejectedPostsAsync(
        int pageNumber,
        int pageSize,
        string? status,
        string? keyword,
        int? categoryId,
        string? brand,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.User)
            .Include(p => p.PostStaffAssignments)
                .ThenInclude(psa => psa.Staff)
            .Where(p => p.Status == "APPROVED" || p.Status == "DENIED");
        
        // Note: ApprovedBy và RejectedBy là Guid?, không có navigation property
        // Sẽ load thông tin admin riêng trong service nếu cần

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusUpper = status.ToUpper();
            if (statusUpper == "APPROVED")
            {
                query = query.Where(p => p.Status == "APPROVED");
            }
            else if (statusUpper == "DENIED")
            {
                query = query.Where(p => p.Status == "DENIED");
            }
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Title.Contains(keyword) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
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
            // Default: sort by approvedAt or rejectedAt desc, then by createdAt desc
            query = query.OrderByDescending(p => p.ApprovedAt ?? p.RejectedAt ?? p.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        return await Task.FromResult(post);
    }

    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid postId)
    {
        return await _context.Posts.AnyAsync(p => p.Id == postId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Post>> GetApprovedPostsAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? brand,
        string? model,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        int? minProductionYear,
        int? maxProductionYear,
        decimal? minBatteryCapacity,
        decimal? maxBatteryCapacity,
        int? minMileage,
        int? maxMileage,
        string? condition,
        bool? auctionOnly,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.User)
            .Include(p => p.PostSubscriptions)
                .ThenInclude(ps => ps.Package)
            .Where(p => p.Status == "APPROVED" && p.IsActive && !p.IsSold);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Title.Contains(keyword) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            query = query.Where(p => p.Model == model);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(p => p.Location != null && p.Location.Contains(location));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (minProductionYear.HasValue)
        {
            query = query.Where(p => p.ProductionYear >= minProductionYear.Value);
        }

        if (maxProductionYear.HasValue)
        {
            query = query.Where(p => p.ProductionYear <= maxProductionYear.Value);
        }

        if (minBatteryCapacity.HasValue)
        {
            query = query.Where(p => p.BatteryCapacityCurrent >= minBatteryCapacity.Value);
        }

        if (maxBatteryCapacity.HasValue)
        {
            query = query.Where(p => p.BatteryCapacityCurrent <= maxBatteryCapacity.Value);
        }

        if (minMileage.HasValue)
        {
            query = query.Where(p => p.Mileage != null && p.Mileage >= minMileage.Value);
        }

        if (maxMileage.HasValue)
        {
            query = query.Where(p => p.Mileage != null && p.Mileage <= maxMileage.Value);
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            query = query.Where(p => p.Condition == condition);
        }

        if (auctionOnly == true)
        {
            query = query.Where(p => p.AuctionEnabled);
        }

        // Check if we need to sort by PriorityLevel (popularity)
        var sortByLower = string.IsNullOrWhiteSpace(sortBy) ? string.Empty : sortBy.ToLower();
        var needsPrioritySort = string.IsNullOrWhiteSpace(sortBy) || 
                                sortByLower == "prioritylevel" || 
                                sortByLower == "popularity";

        if (needsPrioritySort)
        {
            // Materialize query to memory for sorting by PriorityLevel
            var totalCount = await query.CountAsync();
            var posts = await query.ToListAsync();

            // Sort by PriorityLevel desc (highest priority first), then by bumpedAt desc
            var prioritySortDesc = string.IsNullOrWhiteSpace(sortBy) || 
                                   (string.IsNullOrWhiteSpace(sortDirection) || sortDirection.ToLower() == "desc");
            
            var sortedPosts = prioritySortDesc
                ? posts.OrderByDescending(p => 
                    p.PostSubscriptions
                        .OrderByDescending(ps => ps.AppliedAt)
                        .FirstOrDefault()?.Package?.PriorityLevel ?? 0)
                    .ThenByDescending(p => p.BumpedAt)
                : posts.OrderBy(p => 
                    p.PostSubscriptions
                        .OrderByDescending(ps => ps.AppliedAt)
                        .FirstOrDefault()?.Package?.PriorityLevel ?? 0)
                    .ThenByDescending(p => p.BumpedAt);

            // Apply pagination
            var items = sortedPosts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
        }
        else
        {
            // Use EF Core sorting for other fields (more efficient)
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
        }
    }

    public async Task<PagedResult<Post>> GetPostsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? status,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Include(p => p.PostSubscriptions)
                .ThenInclude(ps => ps.Package)
            .Where(p => p.UserId == userId);

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Title.Contains(keyword) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
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
            // Default: sort by createdAt desc (newest first)
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Post>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Post>> GetApprovedPostsByIdsAsync(List<Guid> postIds)
    {
        return await _context.Posts
            .AsNoTracking()
            .Include(p => p.PostImages)
            .Include(p => p.Category)
            .Where(p => postIds.Contains(p.Id) 
                && p.Status == "APPROVED" 
                && p.IsActive == true 
                && p.IsSold == false)
            .ToListAsync();
    }

    private static Expression<Func<Post, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "price" => p => p.Price,
            "createdat" => p => p.CreatedAt,
            "approvedat" => p => p.ApprovedAt ?? DateTime.MinValue,
            "rejectedat" => p => p.RejectedAt ?? DateTime.MinValue,
            "title" => p => p.Title,
            "bumpedat" => p => p.BumpedAt,
            // "approvedat" is also used as "ngày đăng" (post date) - ngày bài đăng được duyệt và hiển thị
            // Note: "prioritylevel" and "popularity" sorting is handled separately in GetApprovedPostsAsync
            // because it requires in-memory sorting after loading PostSubscriptions
            _ => p => p.ApprovedAt ?? p.CreatedAt // Default: sort by approved date or created date
        };
    }
}

