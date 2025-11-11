using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Post Repository
/// </summary>
public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId);
    Task<Post?> GetByIdForUpdateAsync(Guid postId); // Có tracking để update
    Task<Post?> GetByIdForUpdateWithImagesAsync(Guid postId); // Có tracking và include images để update
    Task<Post?> GetByIdWithImagesAsync(Guid postId);
    Task<Post?> GetByIdWithDetailsAsync(Guid postId); // Bao gồm images, category, user, staff assignment, subscription
    Task<PagedResult<Post>> GetPendingPostsAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? brand,
        string? sortBy,
        string? sortDirection);
    Task<PagedResult<Post>> GetApprovedRejectedPostsAsync(
        int pageNumber,
        int pageSize,
        string? status,
        string? keyword,
        int? categoryId,
        string? brand,
        string? sortBy,
        string? sortDirection);
    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<bool> ExistsAsync(Guid postId);
    Task SaveChangesAsync();
    Task<PagedResult<Post>> GetApprovedPostsAsync(
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
        string? sortDirection);
    
    Task<PagedResult<Post>> GetPostsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? status,
        string? sortBy,
        string? sortDirection);
    
    // UC20: So sánh Sản phẩm - Lấy nhiều posts theo IDs (chỉ lấy posts đã được duyệt và đang hoạt động)
    Task<List<Post>> GetApprovedPostsByIdsAsync(List<Guid> postIds);
}

