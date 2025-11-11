using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Bid entity
/// </summary>
public interface IBidRepository
{
    /// <summary>
    /// Lấy bid theo ID
    /// </summary>
    Task<Bid?> GetByIdAsync(Guid bidId);

    /// <summary>
    /// Lấy tất cả bids của một post
    /// </summary>
    Task<List<Bid>> GetBidsByPostIdAsync(Guid postId);

    /// <summary>
    /// Lấy bid cao nhất của một post
    /// </summary>
    Task<Bid?> GetHighestBidByPostIdAsync(Guid postId);

    /// <summary>
    /// Lấy tổng số bids của một post
    /// </summary>
    Task<int> GetBidCountByPostIdAsync(Guid postId);

    /// <summary>
    /// Tạo bid mới
    /// </summary>
    Task<Bid> CreateAsync(Bid bid);

    /// <summary>
    /// Cập nhật bid
    /// </summary>
    Task<Bid> UpdateAsync(Bid bid);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

