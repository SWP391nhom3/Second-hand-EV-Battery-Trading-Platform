using EVehicle.Application.DTOs.Bids;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Bid operations
/// </summary>
public interface IBidService
{
    /// <summary>
    /// UC21: Đặt giá đấu
    /// </summary>
    Task<BaseResponse<BidResponse>> CreateBidAsync(
        Guid userId,
        BidCreateRequest request);

    /// <summary>
    /// UC21: Lấy danh sách bids của một post
    /// </summary>
    Task<BaseResponse<BidListResponse>> GetBidsByPostIdAsync(Guid postId);
}

