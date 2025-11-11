using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho RatingReply entity
/// </summary>
public interface IRatingReplyRepository
{
    /// <summary>
    /// Lấy RatingReply theo ID
    /// </summary>
    Task<RatingReply?> GetByIdAsync(Guid replyId);

    /// <summary>
    /// Lấy danh sách RatingReply theo RatingId
    /// </summary>
    Task<List<RatingReply>> GetRepliesByRatingIdAsync(Guid ratingId);

    /// <summary>
    /// Kiểm tra user đã phản hồi rating chưa
    /// </summary>
    Task<bool> UserHasRepliedAsync(Guid ratingId, Guid userId);

    /// <summary>
    /// Tạo RatingReply mới
    /// </summary>
    Task<RatingReply> CreateAsync(RatingReply reply);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}


