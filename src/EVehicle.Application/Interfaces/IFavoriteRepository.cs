using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Favorite Repository
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>
    /// Lấy favorite theo ID
    /// </summary>
    Task<Favorite?> GetByIdAsync(Guid favoriteId);

    /// <summary>
    /// Lấy favorite theo UserId và PostId (không tracking)
    /// </summary>
    Task<Favorite?> GetByUserAndPostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Lấy favorite theo UserId và PostId (có tracking để xóa)
    /// </summary>
    Task<Favorite?> GetByUserAndPostForDeleteAsync(Guid userId, Guid postId);

    /// <summary>
    /// Kiểm tra xem user đã thêm post vào yêu thích chưa
    /// </summary>
    Task<bool> ExistsAsync(Guid userId, Guid postId);

    /// <summary>
    /// Lấy danh sách favorites của user với phân trang và lọc
    /// </summary>
    Task<PagedResult<Favorite>> GetFavoritesByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? keyword,
        int? categoryId,
        string? status,
        bool? isActive,
        bool? isSold,
        string? sortBy,
        string? sortDirection);

    /// <summary>
    /// Tạo favorite mới
    /// </summary>
    Task<Favorite> CreateAsync(Favorite favorite);

    /// <summary>
    /// Xóa favorite
    /// </summary>
    Task DeleteAsync(Favorite favorite);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

