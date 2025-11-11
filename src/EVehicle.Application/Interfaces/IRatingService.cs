using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Ratings;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Rating operations
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// UC31: Người mua đánh giá người bán
    /// </summary>
    Task<BaseResponse<RatingResponse>> RateSellerAsync(
        Guid buyerId,
        RatingCreateRequest request);

    /// <summary>
    /// UC32: Người bán đánh giá người mua
    /// </summary>
    Task<BaseResponse<RatingResponse>> RateBuyerAsync(
        Guid sellerId,
        RatingCreateRequest request);

    /// <summary>
    /// UC33: Chỉnh sửa đánh giá (trong vòng 7 ngày)
    /// </summary>
    Task<BaseResponse<RatingResponse>> UpdateRatingAsync(
        Guid userId,
        Guid ratingId,
        RatingUpdateRequest request);

    /// <summary>
    /// UC34: Phản hồi đánh giá
    /// </summary>
    Task<BaseResponse<RatingReplyResponse>> ReplyToRatingAsync(
        Guid userId,
        Guid ratingId,
        RatingReplyRequest request);

    /// <summary>
    /// Lấy chi tiết đánh giá
    /// </summary>
    Task<BaseResponse<RatingResponse>> GetRatingByIdAsync(Guid ratingId);

    /// <summary>
    /// Lấy danh sách đánh giá (có phân trang)
    /// </summary>
    Task<PagedResponse<RatingResponse>> GetRatingsAsync(RatingSearchRequest request);
}


