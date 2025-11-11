using EVehicle.Application.DTOs.Bids;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Bid Service implementation
/// </summary>
public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IPostStaffAssignmentRepository _postStaffAssignmentRepository;
    private readonly ILogger<BidService> _logger;

    public BidService(
        IBidRepository bidRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ILeadRepository leadRepository,
        IPostStaffAssignmentRepository postStaffAssignmentRepository,
        ILogger<BidService> logger)
    {
        _bidRepository = bidRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _leadRepository = leadRepository;
        _postStaffAssignmentRepository = postStaffAssignmentRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<BidResponse>> CreateBidAsync(
        Guid userId,
        BidCreateRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Validate post exists and is approved
            var post = await _postRepository.GetByIdForUpdateAsync(request.PostId);
            if (post == null)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            if (post.Status != "APPROVED")
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Bài đăng chưa được duyệt, không thể đấu giá");
            }

            // 3. Validate post has auction enabled
            if (!post.AuctionEnabled)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Bài đăng này không có chế độ đấu giá");
            }

            // 4. Validate auction hasn't ended
            if (post.AuctionEndTime.HasValue && post.AuctionEndTime.Value <= DateTime.UtcNow)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Đấu giá đã kết thúc");
            }

            // 5. Validate post is not sold
            if (post.IsSold)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Sản phẩm đã được bán");
            }

            // 6. Validate user is not the seller
            if (post.UserId == userId)
            {
                return BaseResponse<BidResponse>.FailureResponse(
                    "Bạn không thể đấu giá cho sản phẩm của chính mình");
            }

            // 7. Get current highest bid
            var currentHighestBid = await _bidRepository.GetHighestBidByPostIdAsync(request.PostId);
            
            // 8. Calculate minimum bid amount
            decimal minimumBidAmount;
            if (currentHighestBid != null)
            {
                // Phải cao hơn giá cao nhất hiện tại
                minimumBidAmount = currentHighestBid.BidAmount;
            }
            else
            {
                // Phải cao hơn giá khởi điểm (hoặc giá bài đăng nếu không có startingBid)
                minimumBidAmount = post.StartingBid ?? post.Price;
            }

            // 9. Check buy now price và validate bid amount
            bool isBuyNow = false;
            if (post.BuyNowPrice.HasValue)
            {
                // Kiểm tra xem giá cao nhất hiện tại có đạt giá mua ngay chưa
                // Nếu đã đạt, không thể đấu giá nữa (đã có người mua ngay)
                if (minimumBidAmount >= post.BuyNowPrice.Value)
                {
                    return BaseResponse<BidResponse>.FailureResponse(
                        $"Không thể đấu giá. Đấu giá đã kết thúc vì đã có người mua ngay với giá {post.BuyNowPrice.Value:N0} VNĐ.");
                }
                
                // Kiểm tra xem giá đấu có đạt giá mua ngay không
                // Nếu giá đấu >= giá mua ngay, tự động chuyển sang "mua ngay" và kết thúc đấu giá
                if (request.BidAmount >= post.BuyNowPrice.Value)
                {
                    isBuyNow = true;
                    _logger.LogInformation(
                        "Người dùng {UserId} đạt giá mua ngay {BuyNowPrice} VNĐ cho bài đăng {PostId}. Đấu giá sẽ tự động kết thúc.",
                        userId,
                        post.BuyNowPrice.Value,
                        request.PostId);
                }
                else
                {
                    // Giá đấu thấp hơn giá mua ngay, validate như bình thường
                    if (request.BidAmount <= minimumBidAmount)
                    {
                        return BaseResponse<BidResponse>.FailureResponse(
                            $"Giá đấu phải cao hơn {minimumBidAmount:N0} VNĐ " +
                            $"(tối đa {(post.BuyNowPrice.Value - 1):N0} VNĐ, hoặc {post.BuyNowPrice.Value:N0} VNĐ để mua ngay).");
                    }
                }
            }
            else
            {
                // Không có giá mua ngay, validate như bình thường
                if (request.BidAmount <= minimumBidAmount)
                {
                    return BaseResponse<BidResponse>.FailureResponse(
                        $"Giá đấu phải cao hơn {minimumBidAmount:N0} VNĐ.");
                }
            }

            // 10. Lấy danh sách bidders trước đó (TRƯỚC KHI tạo bid mới)
            var previousBids = await _bidRepository.GetBidsByPostIdAsync(request.PostId);
            var previousBidderIds = previousBids
                .Where(b => b.UserId != userId && b.UserId != post.UserId)
                .Select(b => b.UserId)
                .Distinct()
                .ToList();

            // 11. Create bid
            var bid = new Bid
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                UserId = userId,
                BidAmount = isBuyNow ? post.BuyNowPrice!.Value : request.BidAmount, // Nếu mua ngay, dùng giá mua ngay
                IsWinningBid = isBuyNow, // Nếu mua ngay, tự động thắng
                CreatedAt = DateTime.UtcNow
            };

            await _bidRepository.CreateAsync(bid);
            
            // 12. Nếu là mua ngay, kết thúc đấu giá và tạo Lead
            Guid? leadId = null;
            if (isBuyNow)
            {
                // Kết thúc đấu giá (set AuctionEndTime = now)
                post.AuctionEndTime = DateTime.UtcNow;
                await _postRepository.UpdateAsync(post);
                
                // Kiểm tra xem Post đã có Staff được gán chưa
                var postWithDetails = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
                Guid? assignedStaffId = null;
                Guid? assignedByAdminId = null;
                string leadStatus = "NEW";
                DateTime? assignedAt = null;

                if (postWithDetails != null)
                {
                    var activeStaffAssignment = postWithDetails.PostStaffAssignments
                        .FirstOrDefault(psa => psa.IsActive);

                    if (activeStaffAssignment != null)
                    {
                        // Nếu Post đã có Staff, tự động gán Staff đó cho Lead
                        assignedStaffId = activeStaffAssignment.StaffId;
                        assignedByAdminId = activeStaffAssignment.AssignedBy;
                        leadStatus = "ASSIGNED";
                        assignedAt = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Tự động gán Staff {StaffId} cho Lead AUCTION_WINNER từ Post {PostId}",
                            assignedStaffId,
                            request.PostId);
                    }
                    else
                    {
                        // Nếu Post chưa có Staff, tự động gán Staff cho Post và Lead
                        var staffList = await _userRepository.GetStaffAsync();
                        var admins = await _userRepository.GetAdminsAsync();
                        
                        if (staffList != null && staffList.Any() && admins != null && admins.Any())
                        {
                            // Chọn staff đầu tiên (có thể cải thiện logic này sau - round-robin hoặc dựa trên workload)
                            var selectedStaff = staffList.First();
                            assignedStaffId = selectedStaff.Id;
                            
                            // Lấy admin đầu tiên để làm AssignedBy (system auto-assign)
                            assignedByAdminId = admins.First().Id;
                            
                            // Tạo PostStaffAssignment
                            var postStaffAssignment = new PostStaffAssignment
                            {
                                Id = Guid.NewGuid(),
                                PostId = request.PostId,
                                StaffId = selectedStaff.Id,
                                AssignedBy = assignedByAdminId.Value,
                                AssignedAt = DateTime.UtcNow,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            await _postStaffAssignmentRepository.CreateAsync(postStaffAssignment);
                            await _postStaffAssignmentRepository.SaveChangesAsync();
                            
                            leadStatus = "ASSIGNED";
                            assignedAt = DateTime.UtcNow;
                            
                            _logger.LogInformation(
                                "Tự động gán Staff {StaffId} cho Post {PostId} và Lead AUCTION_WINNER (khi có người mua ngay), AssignedBy: {AdminId}",
                                assignedStaffId,
                                request.PostId,
                                assignedByAdminId);
                        }
                        else
                        {
                            // Không có staff hoặc admin available, để Lead ở trạng thái NEW và Admin sẽ gán sau
                            _logger.LogWarning(
                                "Không có Staff hoặc Admin available để gán cho Post {PostId}. Lead AUCTION_WINNER sẽ ở trạng thái NEW và Admin sẽ gán sau.",
                                request.PostId);
                        }
                    }
                }

                // Kiểm tra xem đã có Lead AUCTION_WINNER cho Post này chưa
                var existingLead = await _leadRepository.GetByPostIdAndBuyerIdAsync(request.PostId, userId);
                if (existingLead != null && existingLead.LeadType == "AUCTION_WINNER")
                {
                    // Đã có Lead, không tạo mới (trường hợp hiếm)
                    leadId = existingLead.Id;
                    _logger.LogWarning(
                        "Đã tồn tại Lead AUCTION_WINNER cho Post {PostId} và Buyer {UserId}",
                        request.PostId,
                        userId);
                }
                else
                {
                    // Tạo Lead với loại AUCTION_WINNER
                    var lead = new Lead
                    {
                        Id = Guid.NewGuid(),
                        PostId = request.PostId,
                        BuyerId = userId,
                        StaffId = assignedStaffId,
                        AssignedBy = assignedByAdminId,
                        LeadType = "AUCTION_WINNER",
                        Status = leadStatus,
                        FinalPrice = post.BuyNowPrice!.Value, // Giá mua ngay
                        AssignedAt = assignedAt,
                        ClosedAt = null,
                        Notes = null,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _leadRepository.CreateAsync(lead);
                    leadId = lead.Id;
                    
                    _logger.LogInformation(
                        "Đã tạo Lead AUCTION_WINNER {LeadId} cho Buyer {UserId} với giá {FinalPrice} VNĐ",
                        lead.Id,
                        userId,
                        lead.FinalPrice);
                }
            }

            // 13. Create notifications
            // Notify seller
            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = post.UserId,
                NotificationType = isBuyNow ? "AUCTION_ENDED" : "NEW_BID",
                Title = isBuyNow ? "Đấu giá đã kết thúc - Có người mua ngay" : "Có người đấu giá mới",
                Content = isBuyNow 
                    ? $"Người dùng {user.FullName ?? user.Email} đã mua ngay sản phẩm \"{post.Title}\" với giá {post.BuyNowPrice!.Value:N0} VNĐ. Đấu giá đã kết thúc."
                    : $"Có người đặt giá {bid.BidAmount:N0} VNĐ cho bài đăng \"{post.Title}\".",
                RelatedId = request.PostId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(sellerNotification);

            // Notify other bidders
            if (isBuyNow)
            {
                // Nếu là mua ngay, thông báo cho tất cả bidders rằng đấu giá đã kết thúc
                foreach (var previousBidderId in previousBidderIds)
                {
                    var bidderNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = previousBidderId,
                        NotificationType = "AUCTION_ENDED",
                        Title = "Đấu giá đã kết thúc",
                        Content = $"Đấu giá cho bài đăng \"{post.Title}\" đã kết thúc vì có người mua ngay với giá {post.BuyNowPrice!.Value:N0} VNĐ.",
                        RelatedId = request.PostId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(bidderNotification);
                }
                
                // Gửi thông báo cho Admin hoặc Staff (nếu có Lead)
                if (leadId.HasValue)
                {
                    var postWithDetails = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
                    Guid? assignedStaffId = null;
                    
                    if (postWithDetails != null)
                    {
                        var activeStaffAssignment = postWithDetails.PostStaffAssignments
                            .FirstOrDefault(psa => psa.IsActive);
                        assignedStaffId = activeStaffAssignment?.StaffId;
                    }
                    
                    if (!assignedStaffId.HasValue)
                    {
                        // Gửi thông báo cho Admin
                        var adminUsers = await _userRepository.GetAdminsAsync();
                        foreach (var admin in adminUsers)
                        {
                            var adminNotification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                UserId = admin.Id,
                                NotificationType = "NEW_LEAD",
                                Title = "Có người thắng đấu giá (Mua ngay)",
                                Content = $"Người dùng {user.FullName ?? user.Email} đã mua ngay sản phẩm \"{post.Title}\" với giá {post.BuyNowPrice.Value:N0} VNĐ. Vui lòng gán Staff để hỗ trợ.",
                                RelatedId = leadId.Value,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _notificationRepository.CreateAsync(adminNotification);
                        }
                    }
                    else
                    {
                        // Gửi thông báo cho Staff được gán
                        var staffNotification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = assignedStaffId.Value,
                            NotificationType = "LEAD_ASSIGNED",
                            Title = "Bạn được gán Lead đấu giá (Mua ngay)",
                            Content = $"Người dùng {user.FullName ?? user.Email} đã mua ngay sản phẩm \"{post.Title}\" với giá {post.BuyNowPrice.Value:N0} VNĐ. Vui lòng liên hệ để hỗ trợ.",
                            RelatedId = leadId.Value,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.CreateAsync(staffNotification);
                    }
                }
            }
            else
            {
                // Nếu không phải mua ngay, thông báo như bình thường
                foreach (var previousBidderId in previousBidderIds)
                {
                    var bidderNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = previousBidderId,
                        NotificationType = "BID_OUTBID",
                        Title = "Bạn đã bị vượt giá",
                        Content = $"Có người đặt giá {bid.BidAmount:N0} VNĐ cao hơn bạn cho bài đăng \"{post.Title}\".",
                        RelatedId = request.PostId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(bidderNotification);
                }
            }

            // 14. Save all changes (save sau khi tạo tất cả notifications)
            await _bidRepository.SaveChangesAsync();
            await _postRepository.SaveChangesAsync();
            if (isBuyNow)
            {
                await _leadRepository.SaveChangesAsync();
            }
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Người dùng {UserId} đã đặt giá đấu {BidAmount} cho bài đăng {PostId}. IsBuyNow: {IsBuyNow}",
                userId,
                bid.BidAmount,
                request.PostId,
                isBuyNow);

            // 15. Map to response
            var response = MapToResponse(bid, user);

            var message = isBuyNow 
                ? $"Mua ngay thành công! Bạn đã thắng đấu giá với giá {post.BuyNowPrice!.Value:N0} VNĐ. Staff sẽ liên hệ để hỗ trợ bạn."
                : "Đặt giá đấu thành công";

            return BaseResponse<BidResponse>.SuccessResponse(
                response,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đặt giá đấu, UserId: {UserId}, PostId: {PostId}", userId, request.PostId);
            return BaseResponse<BidResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đặt giá đấu");
        }
    }

    public async Task<BaseResponse<BidListResponse>> GetBidsByPostIdAsync(Guid postId)
    {
        try
        {
            // 1. Validate post exists
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return BaseResponse<BidListResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 2. Get bids
            var bids = await _bidRepository.GetBidsByPostIdAsync(postId);
            var totalBids = await _bidRepository.GetBidCountByPostIdAsync(postId);
            var highestBid = await _bidRepository.GetHighestBidByPostIdAsync(postId);

            // 3. Map to response
            var response = new BidListResponse
            {
                PostId = postId,
                StartingBid = post.StartingBid,
                BuyNowPrice = post.BuyNowPrice,
                AuctionEndTime = post.AuctionEndTime,
                CurrentHighestBid = highestBid?.BidAmount,
                TotalBids = totalBids,
                Bids = bids.Select(b => MapToResponse(b, b.User)).ToList()
            };

            return BaseResponse<BidListResponse>.SuccessResponse(
                response,
                "Lấy danh sách đấu giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đấu giá, PostId: {PostId}", postId);
            return BaseResponse<BidListResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách đấu giá");
        }
    }

    private BidResponse MapToResponse(Bid bid, Domain.Entities.User user)
    {
        return new BidResponse
        {
            BidId = bid.Id,
            PostId = bid.PostId,
            UserId = bid.UserId,
            BidderName = user.FullName ?? user.Email,
            BidAmount = bid.BidAmount,
            IsWinningBid = bid.IsWinningBid,
            CreatedAt = bid.CreatedAt
        };
    }
}

