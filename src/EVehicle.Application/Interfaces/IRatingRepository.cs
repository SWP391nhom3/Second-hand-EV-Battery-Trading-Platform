using EVehicle.Domain.Entities;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Rating entity
/// </summary>
public interface IRatingRepository
{
    /// <summary>
    /// Lấy Rating theo ID
    /// </summary>
    Task<Rating?> GetByIdAsync(Guid ratingId);

    /// <summary>
    /// Lấy Rating theo ID (có tracking để update)
    /// </summary>
    Task<Rating?> GetByIdForUpdateAsync(Guid ratingId);

    /// <summary>
    /// Lấy Rating theo ID với đầy đủ thông tin (bao gồm replies)
    /// </summary>
    Task<Rating?> GetByIdWithDetailsAsync(Guid ratingId);

    /// <summary>
    /// Lấy Rating theo OrderId, RaterId, RateeId
    /// </summary>
    Task<Rating?> GetByOrderAndUsersAsync(Guid orderId, Guid raterId, Guid rateeId);

    /// <summary>
    /// Lấy danh sách Rating theo RateeId (người được đánh giá)
    /// </summary>
    Task<PagedResult<Rating>> GetRatingsByRateeIdAsync(
        Guid rateeId,
        string? rateeRole,
        int? minScore,
        int? maxScore,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection);

    /// <summary>
    /// Lấy danh sách Rating theo OrderId
    /// </summary>
    Task<List<Rating>> GetRatingsByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Kiểm tra Rating đã tồn tại chưa
    /// </summary>
    Task<bool> ExistsAsync(Guid orderId, Guid raterId, Guid rateeId);

    /// <summary>
    /// Tạo Rating mới
    /// </summary>
    Task<Rating> CreateAsync(Rating rating);

    /// <summary>
    /// Cập nhật Rating
    /// </summary>
    Task<Rating> UpdateAsync(Rating rating);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

