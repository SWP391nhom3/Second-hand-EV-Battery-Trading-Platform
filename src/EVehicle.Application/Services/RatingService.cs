using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Ratings;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Rating Service implementation
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IRatingReplyRepository _ratingReplyRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RatingService> _logger;

    public RatingService(
        IRatingRepository ratingRepository,
        IRatingReplyRepository ratingReplyRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<RatingService> logger)
    {
        _ratingRepository = ratingRepository;
        _ratingReplyRepository = ratingReplyRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// UC31: Người mua đánh giá người bán
    /// </summary>
    public async Task<BaseResponse<RatingResponse>> RateSellerAsync(
        Guid buyerId,
        RatingCreateRequest request)
    {
        try
        {
            // 1. Validate Order exists and is completed
            var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Không tìm thấy đơn hàng");
            }

            // 2. Validate Order is completed
            if (order.Status != "COMPLETED")
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Chỉ có thể đánh giá sau khi giao dịch hoàn tất");
            }

            // 3. Validate buyer is the buyer of this order
            if (order.BuyerId != buyerId)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Bạn không phải người mua của đơn hàng này");
            }

            // 4. Check if rating already exists
            var existingRating = await _ratingRepository.GetByOrderAndUsersAsync(
                request.OrderId, 
                buyerId, 
                order.SellerId);
            
            if (existingRating != null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Bạn đã đánh giá người bán cho đơn hàng này rồi");
            }

            // 5. Create rating
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                RaterId = buyerId,
                RateeId = order.SellerId,
                RateeRole = "SELLER",
                Score = request.Score,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _ratingRepository.CreateAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Buyer {BuyerId} đã đánh giá Seller {SellerId} cho Order {OrderId} với điểm {Score}",
                buyerId,
                order.SellerId,
                request.OrderId,
                request.Score);

            // 6. Get rating with details
            var ratingWithDetails = await _ratingRepository.GetByIdWithDetailsAsync(rating.Id);
            var response = MapToResponse(ratingWithDetails!);

            // 7. Create notification for seller
            var postTitle = order.Post?.Title ?? "sản phẩm";
            await _notificationService.CreateNotificationAsync(
                order.SellerId,
                "RATING_RECEIVED",
                "Bạn đã nhận được đánh giá mới",
                $"Người mua đã đánh giá bạn {request.Score} sao cho đơn hàng \"{postTitle}\". {(!string.IsNullOrEmpty(request.Comment) ? "Nhận xét: " + request.Comment : "")}",
                rating.Id);

            return BaseResponse<RatingResponse>.SuccessResponse(
                response,
                "Đánh giá người bán thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá người bán, BuyerId: {BuyerId}, OrderId: {OrderId}",
                buyerId, request.OrderId);
            return BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh giá người bán");
        }
    }

    /// <summary>
    /// UC32: Người bán đánh giá người mua
    /// </summary>
    public async Task<BaseResponse<RatingResponse>> RateBuyerAsync(
        Guid sellerId,
        RatingCreateRequest request)
    {
        try
        {
            // 1. Validate Order exists and is completed
            var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Không tìm thấy đơn hàng");
            }

            // 2. Validate Order is completed
            if (order.Status != "COMPLETED")
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Chỉ có thể đánh giá sau khi giao dịch hoàn tất");
            }

            // 3. Validate seller is the seller of this order
            if (order.SellerId != sellerId)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Bạn không phải người bán của đơn hàng này");
            }

            // 4. Check if rating already exists
            var existingRating = await _ratingRepository.GetByOrderAndUsersAsync(
                request.OrderId, 
                sellerId, 
                order.BuyerId);
            
            if (existingRating != null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Bạn đã đánh giá người mua cho đơn hàng này rồi");
            }

            // 5. Create rating
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                RaterId = sellerId,
                RateeId = order.BuyerId,
                RateeRole = "BUYER",
                Score = request.Score,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _ratingRepository.CreateAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Seller {SellerId} đã đánh giá Buyer {BuyerId} cho Order {OrderId} với điểm {Score}",
                sellerId,
                order.BuyerId,
                request.OrderId,
                request.Score);

            // 6. Get rating with details
            var ratingWithDetails = await _ratingRepository.GetByIdWithDetailsAsync(rating.Id);
            var response = MapToResponse(ratingWithDetails!);

            // 7. Create notification for buyer
            var postTitle = order.Post?.Title ?? "sản phẩm";
            await _notificationService.CreateNotificationAsync(
                order.BuyerId,
                "RATING_RECEIVED",
                "Bạn đã nhận được đánh giá mới",
                $"Người bán đã đánh giá bạn {request.Score} sao cho đơn hàng \"{postTitle}\". {(!string.IsNullOrEmpty(request.Comment) ? "Nhận xét: " + request.Comment : "")}",
                rating.Id);

            return BaseResponse<RatingResponse>.SuccessResponse(
                response,
                "Đánh giá người mua thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá người mua, SellerId: {SellerId}, OrderId: {OrderId}",
                sellerId, request.OrderId);
            return BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh giá người mua");
        }
    }

    /// <summary>
    /// UC33: Chỉnh sửa đánh giá (trong vòng 7 ngày)
    /// </summary>
    public async Task<BaseResponse<RatingResponse>> UpdateRatingAsync(
        Guid userId,
        Guid ratingId,
        RatingUpdateRequest request)
    {
        try
        {
            // 1. Get rating (with tracking for update)
            var rating = await _ratingRepository.GetByIdForUpdateAsync(ratingId);
            if (rating == null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Không tìm thấy đánh giá");
            }

            // 2. Validate user is the rater
            if (rating.RaterId != userId)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Bạn không có quyền chỉnh sửa đánh giá này");
            }

            // 3. Check if rating can be edited (within 7 days from creation)
            var daysSinceCreation = (DateTime.UtcNow - rating.CreatedAt).TotalDays;
            if (daysSinceCreation > 7)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Chỉ có thể chỉnh sửa đánh giá trong vòng 7 ngày kể từ ngày tạo");
            }

            // 4. Update rating
            rating.Score = request.Score;
            rating.Comment = request.Comment;
            rating.UpdatedAt = DateTime.UtcNow;

            await _ratingRepository.UpdateAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} đã chỉnh sửa Rating {RatingId}",
                userId,
                ratingId);

            // 5. Get rating with details
            var ratingWithDetails = await _ratingRepository.GetByIdWithDetailsAsync(rating.Id);
            var response = MapToResponse(ratingWithDetails!);

            // 6. Create notification for ratee (người được đánh giá) when rating is updated
            var order = await _orderRepository.GetByIdWithDetailsAsync(rating.OrderId);
            if (order != null)
            {
                var postTitle = order.Post?.Title ?? "sản phẩm";
                await _notificationService.CreateNotificationAsync(
                    rating.RateeId,
                    "RATING_UPDATED",
                    "Đánh giá về bạn đã được chỉnh sửa",
                    $"Đánh giá về bạn cho đơn hàng \"{postTitle}\" đã được chỉnh sửa thành {request.Score} sao. {(!string.IsNullOrEmpty(request.Comment) ? "Nhận xét mới: " + request.Comment : "")}",
                    rating.Id);
            }

            return BaseResponse<RatingResponse>.SuccessResponse(
                response,
                "Chỉnh sửa đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chỉnh sửa đánh giá, UserId: {UserId}, RatingId: {RatingId}",
                userId, ratingId);
            return BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi chỉnh sửa đánh giá");
        }
    }

    /// <summary>
    /// UC34: Phản hồi đánh giá
    /// </summary>
    public async Task<BaseResponse<RatingReplyResponse>> ReplyToRatingAsync(
        Guid userId,
        Guid ratingId,
        RatingReplyRequest request)
    {
        try
        {
            // 1. Get rating (no tracking needed for read)
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Không tìm thấy đánh giá");
            }

            // 2. Validate user is the ratee (người được đánh giá)
            if (rating.RateeId != userId)
            {
                return BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Bạn không phải người được đánh giá, không thể phản hồi");
            }

            // 3. Check if user has already replied
            var hasReplied = await _ratingReplyRepository.UserHasRepliedAsync(ratingId, userId);
            if (hasReplied)
            {
                return BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Bạn đã phản hồi đánh giá này rồi");
            }

            // 4. Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<RatingReplyResponse>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            // 5. Create reply
            var reply = new RatingReply
            {
                Id = Guid.NewGuid(),
                RatingId = ratingId,
                UserId = userId,
                ReplyContent = request.ReplyContent,
                CreatedAt = DateTime.UtcNow
            };

            await _ratingReplyRepository.CreateAsync(reply);
            await _ratingReplyRepository.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} đã phản hồi Rating {RatingId}",
                userId,
                ratingId);

            // 6. Get reply with details
            var replyWithDetails = await _ratingReplyRepository.GetByIdAsync(reply.Id);
            var response = MapToReplyResponse(replyWithDetails!);

            // 7. Create notification for rater (người đánh giá) when there's a reply
            var order = await _orderRepository.GetByIdWithDetailsAsync(rating.OrderId);
            if (order != null)
            {
                var postTitle = order.Post?.Title ?? "sản phẩm";
                var rateeName = user.FullName ?? user.Email ?? "Người dùng";
                await _notificationService.CreateNotificationAsync(
                    rating.RaterId,
                    "RATING_REPLY",
                    "Bạn đã nhận được phản hồi đánh giá",
                    $"{rateeName} đã phản hồi đánh giá của bạn cho đơn hàng \"{postTitle}\": {request.ReplyContent}",
                    rating.Id);
            }

            return BaseResponse<RatingReplyResponse>.SuccessResponse(
                response,
                "Phản hồi đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phản hồi đánh giá, UserId: {UserId}, RatingId: {RatingId}",
                userId, ratingId);
            return BaseResponse<RatingReplyResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi phản hồi đánh giá");
        }
    }

    /// <summary>
    /// Lấy chi tiết đánh giá
    /// </summary>
    public async Task<BaseResponse<RatingResponse>> GetRatingByIdAsync(Guid ratingId)
    {
        try
        {
            var rating = await _ratingRepository.GetByIdWithDetailsAsync(ratingId);
            if (rating == null)
            {
                return BaseResponse<RatingResponse>.FailureResponse(
                    "Không tìm thấy đánh giá");
            }

            var response = MapToResponse(rating);
            return BaseResponse<RatingResponse>.SuccessResponse(
                response,
                "Lấy chi tiết đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết đánh giá, RatingId: {RatingId}", ratingId);
            return BaseResponse<RatingResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết đánh giá");
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá (có phân trang)
    /// </summary>
    public async Task<PagedResponse<RatingResponse>> GetRatingsAsync(RatingSearchRequest request)
    {
        try
        {
            request.IsValid();

            // Validate RateeId or OrderId is provided
            if (!request.RateeId.HasValue && !request.OrderId.HasValue)
            {
                return PagedResponse<RatingResponse>.FailureResponse(
                    "Phải cung cấp RateeId hoặc OrderId");
            }

            List<Rating> ratings;
            int totalCount;

            if (request.OrderId.HasValue)
            {
                // Get ratings by OrderId
                ratings = await _ratingRepository.GetRatingsByOrderIdAsync(request.OrderId.Value);
                totalCount = ratings.Count;

                // Apply pagination manually for OrderId case
                ratings = ratings
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
            }
            else
            {
                // Get ratings by RateeId with filters
                var result = await _ratingRepository.GetRatingsByRateeIdAsync(
                    request.RateeId!.Value,
                    request.RateeRole,
                    request.MinScore,
                    request.MaxScore,
                    request.PageNumber,
                    request.PageSize,
                    request.SortBy,
                    request.SortDirection);
                
                ratings = result.Items;
                totalCount = result.TotalCount;
            }

            var responses = ratings.Select(MapToResponse).ToList();

            return PagedResponse<RatingResponse>.SuccessResponse(
                responses,
                request.PageNumber,
                request.PageSize,
                totalCount,
                "Lấy danh sách đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đánh giá");
            return PagedResponse<RatingResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách đánh giá");
        }
    }

    /// <summary>
    /// Map Rating entity to RatingResponse DTO
    /// </summary>
    private RatingResponse MapToResponse(Rating rating)
    {
        // Can edit within 7 days from creation date
        var daysSinceCreation = (DateTime.UtcNow - rating.CreatedAt).TotalDays;
        var canEdit = daysSinceCreation <= 7;

        return new RatingResponse
        {
            RatingId = rating.Id,
            OrderId = rating.OrderId,
            RaterId = rating.RaterId,
            RaterName = rating.Rater?.FullName ?? rating.Rater?.Email ?? "Người dùng",
            RateeId = rating.RateeId,
            RateeName = rating.Ratee?.FullName ?? rating.Ratee?.Email ?? "Người dùng",
            RateeRole = rating.RateeRole,
            Score = rating.Score,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt,
            Replies = rating.RatingReplies?
                .Select(rr => MapToReplyResponse(rr))
                .ToList() ?? new List<RatingReplyResponse>(),
            CanEdit = canEdit
        };
    }

    /// <summary>
    /// Map RatingReply entity to RatingReplyResponse DTO
    /// </summary>
    private RatingReplyResponse MapToReplyResponse(RatingReply reply)
    {
        return new RatingReplyResponse
        {
            ReplyId = reply.Id,
            RatingId = reply.RatingId,
            UserId = reply.UserId,
            UserName = reply.User?.FullName ?? reply.User?.Email ?? "Người dùng",
            ReplyContent = reply.ReplyContent,
            CreatedAt = reply.CreatedAt
        };
    }
}

