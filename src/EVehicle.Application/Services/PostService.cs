using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Post Service implementation
/// </summary>
public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUserPackageCreditsRepository _userPackageCreditsRepository;
    private readonly IPostStaffAssignmentRepository _postStaffAssignmentRepository;
    private readonly IPostSubscriptionRepository _postSubscriptionRepository;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IPackageRepository packageRepository,
        IUserPackageCreditsRepository userPackageCreditsRepository,
        IPostStaffAssignmentRepository postStaffAssignmentRepository,
        IPostSubscriptionRepository postSubscriptionRepository,
        INotificationService notificationService,
        IFileStorageService fileStorageService,
        ILogger<PostService> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _packageRepository = packageRepository;
        _userPackageCreditsRepository = userPackageCreditsRepository;
        _postStaffAssignmentRepository = postStaffAssignmentRepository;
        _postSubscriptionRepository = postSubscriptionRepository;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<BaseResponse<PostResponse>> CreatePostAsync(
        Guid userId,
        PostCreateRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Validate category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Danh mục không tồn tại");
            }

            // 3. Validate package exists
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Gói tin không tồn tại");
            }

            // 4. Check credits (chỉ kiểm tra, chưa trừ credits - sẽ trừ khi Admin duyệt)
            var userCredits = await _userPackageCreditsRepository.GetByUserAndPackageAsync(
                userId,
                request.PackageId);
            
            if (userCredits == null || userCredits.CreditsRemaining <= 0)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    $"Không đủ credits cho gói tin {package.Name}. Vui lòng mua gói tin trước khi đăng bài.");
            }

            // 5. Validate number of images with package max images
            if (request.Images.Count > package.MaxImages)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    $"Gói tin {package.Name} chỉ cho phép tối đa {package.MaxImages} ảnh. Bạn đã tải lên {request.Images.Count} ảnh.");
            }

            // 6. Upload images
            var imageUrls = new List<string>();
            foreach (var image in request.Images)
            {
                var imageUrl = await _fileStorageService.UploadImageAsync(image);
                imageUrls.Add(imageUrl);
            }

            // 7. Upload proof image
            string? proofImageUrl = null;
            if (request.ProofImage != null)
            {
                proofImageUrl = await _fileStorageService.UploadImageAsync(request.ProofImage);
            }

            // 8. TODO: AI gợi ý giá (sẽ implement sau)
            // var suggestedPrice = await _aiService.SuggestPriceAsync(request);

            // 9. Create post entity
            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Location = request.Location,
                Brand = request.Brand,
                Model = request.Model,
                BatteryCapacityCurrent = request.BatteryCapacityCurrent,
                ChargeCount = request.ChargeCount,
                ProductionYear = request.ProductionYear,
                Condition = request.Condition,
                Mileage = request.Mileage,
                SelectedPackageId = request.PackageId, // Lưu packageId đã chọn
                Status = "PENDING", // Chờ Admin duyệt
                IsActive = true,
                IsSold = false,
                CreatedAt = DateTime.UtcNow,
                BumpedAt = DateTime.UtcNow,
                // Auction fields
                AuctionEnabled = request.AuctionEnabled,
                StartingBid = request.StartingBid,
                BuyNowPrice = request.BuyNowPrice,
                AuctionEndTime = request.AuctionEndTime
            };

            // 10. Create post images
            for (int i = 0; i < imageUrls.Count; i++)
            {
                post.PostImages.Add(new PostImage
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ImageUrl = imageUrls[i],
                    IsThumbnail = i == 0, // Ảnh đầu tiên là thumbnail
                    IsProof = false,
                    DisplayOrder = i,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 11. Add proof image
            if (!string.IsNullOrEmpty(proofImageUrl))
            {
                post.PostImages.Add(new PostImage
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ImageUrl = proofImageUrl,
                    IsThumbnail = false,
                    IsProof = true,
                    DisplayOrder = imageUrls.Count,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 12. Save to database
            await _postRepository.CreateAsync(post);
            await _postRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Tạo bài đăng thành công, PostId: {PostId}, UserId: {UserId}, PackageId: {PackageId}",
                post.Id,
                userId,
                request.PackageId);

            // 13. Thông báo cho tất cả Admin về bài đăng mới cần duyệt
            try
            {
                var admins = await _userRepository.GetAdminsAsync();
                foreach (var admin in admins)
                {
                    await _notificationService.CreateNotificationAsync(
                        admin.Id,
                        "NEW_POST_PENDING",
                        "Có bài đăng mới cần duyệt",
                        $"Bài đăng \"{post.Title}\" của người dùng {user.FullName ?? user.Email} đang chờ duyệt.",
                        post.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gửi thông báo cho admin về bài đăng mới, PostId: {PostId}", post.Id);
                // Không throw để không ảnh hưởng đến flow chính
            }

            // 14. Map to response
            var response = MapToResponse(post, category);

            return BaseResponse<PostResponse>.SuccessResponse(
                response,
                "Tạo bài đăng thành công. Bài đăng đang chờ Admin duyệt. Credits sẽ được trừ khi bài đăng được duyệt.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài đăng, UserId: {UserId}", userId);
            return BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo bài đăng");
        }
    }

    public async Task<BaseResponse<PostDetailResponse>> GetPostByIdAsync(Guid postId)
    {
        try
        {
            var post = await _postRepository.GetByIdWithDetailsAsync(postId);
            if (post == null)
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // Load admin info if needed
            var admins = new Dictionary<Guid, Domain.Entities.User>();
            if (post.ApprovedBy.HasValue)
            {
                var approvedByAdmin = await _userRepository.GetByIdAsync(post.ApprovedBy.Value);
                if (approvedByAdmin != null) admins[approvedByAdmin.Id] = approvedByAdmin;
            }
            if (post.RejectedBy.HasValue)
            {
                var rejectedByAdmin = await _userRepository.GetByIdAsync(post.RejectedBy.Value);
                if (rejectedByAdmin != null) admins[rejectedByAdmin.Id] = rejectedByAdmin;
            }
            
            var response = MapToDetailResponse(post, admins);
            return BaseResponse<PostDetailResponse>.SuccessResponse(
                response,
                "Lấy thông tin bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin bài đăng, PostId: {PostId}", postId);
            return BaseResponse<PostDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin bài đăng");
        }
    }

    public async Task<BaseResponse<PostPublicDetailResponse>> GetPostByIdPublicAsync(Guid postId)
    {
        try
        {
            var post = await _postRepository.GetByIdWithDetailsAsync(postId);
            if (post == null)
            {
                return BaseResponse<PostPublicDetailResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // Chỉ cho phép xem bài đăng đã được duyệt
            if (post.Status != "APPROVED")
            {
                return BaseResponse<PostPublicDetailResponse>.FailureResponse(
                    "Bài đăng không khả dụng");
            }

            var response = MapToPublicDetailResponse(post);
            return BaseResponse<PostPublicDetailResponse>.SuccessResponse(
                response,
                "Lấy thông tin bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin bài đăng (public), PostId: {PostId}", postId);
            return BaseResponse<PostPublicDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin bài đăng");
        }
    }

    public async Task<PagedResponse<PostResponse>> SearchApprovedPostsAsync(PostSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _postRepository.GetApprovedPostsAsync(
                request.PageNumber,
                request.PageSize,
                request.Keyword,
                request.CategoryId,
                request.Brand,
                request.Model,
                request.Location,
                request.MinPrice,
                request.MaxPrice,
                request.MinProductionYear,
                request.MaxProductionYear,
                request.MinBatteryCapacity,
                request.MaxBatteryCapacity,
                request.MinMileage,
                request.MaxMileage,
                request.Condition,
                request.AuctionOnly,
                request.SortBy,
                request.SortDirection);

            var responses = result.Items.Select(post => MapToResponse(post, post.Category)).ToList();

            return PagedResponse<PostResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tìm kiếm bài đăng");
            return PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi tìm kiếm bài đăng");
        }
    }

    public async Task<PagedResponse<PendingPostResponse>> GetPendingPostsAsync(
        Guid adminId,
        PendingPostSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _postRepository.GetPendingPostsAsync(
                request.PageNumber,
                request.PageSize,
                request.Keyword,
                request.CategoryId,
                request.Brand,
                request.SortBy,
                request.SortDirection);

            var responses = result.Items.Select(MapToPendingResponse).ToList();

            _logger.LogInformation(
                "Admin {AdminId} lấy danh sách bài đăng chờ duyệt, Total: {TotalCount}",
                adminId,
                result.TotalCount);

            return PagedResponse<PendingPostResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách bài đăng chờ duyệt thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng chờ duyệt");
            return PagedResponse<PendingPostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng chờ duyệt");
        }
    }

    public async Task<PagedResponse<ApprovedRejectedPostResponse>> GetApprovedRejectedPostsAsync(
        Guid adminId,
        ApprovedRejectedPostSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _postRepository.GetApprovedRejectedPostsAsync(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.Keyword,
                request.CategoryId,
                request.Brand,
                request.SortBy,
                request.SortDirection);

            // Load admin info for all posts
            var adminIds = result.Items
                .Where(p => p.ApprovedBy.HasValue || p.RejectedBy.HasValue)
                .SelectMany(p => new[] { p.ApprovedBy, p.RejectedBy })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var admins = new Dictionary<Guid, Domain.Entities.User>();
            if (adminIds.Any())
            {
                var adminUsers = await Task.WhenAll(adminIds.Select(id => _userRepository.GetByIdAsync(id)));
                foreach (var admin in adminUsers.Where(u => u != null))
                {
                    admins[admin!.Id] = admin;
                }
            }

            var responses = result.Items.Select(p => MapToApprovedRejectedResponse(p, admins)).ToList();

            _logger.LogInformation(
                "Admin {AdminId} lấy danh sách bài đăng đã duyệt/từ chối, Status: {Status}, Total: {TotalCount}",
                adminId,
                request.Status ?? "ALL",
                result.TotalCount);

            return PagedResponse<ApprovedRejectedPostResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách bài đăng đã duyệt/từ chối thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng đã duyệt/từ chối");
            return PagedResponse<ApprovedRejectedPostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng đã duyệt/từ chối");
        }
    }

    public async Task<BaseResponse<PostDetailResponse>> ApprovePostAsync(
        Guid adminId,
        Guid postId,
        PostApproveRequest request)
    {
        try
        {
            // 1. Validate post exists and is PENDING (use tracking for update)
            var post = await _postRepository.GetByIdForUpdateAsync(postId);
            if (post == null)
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            if (post.Status != "PENDING")
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    $"Bài đăng không ở trạng thái PENDING. Trạng thái hiện tại: {post.Status}");
            }

            // 2. Validate package exists
            if (!post.SelectedPackageId.HasValue)
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    "Bài đăng không có gói tin đã chọn");
            }

            var package = await _packageRepository.GetByIdAsync(post.SelectedPackageId.Value);
            if (package == null)
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    "Gói tin không tồn tại");
            }

            // 3. Get user credits and validate
            var userCredits = await _userPackageCreditsRepository.GetByUserAndPackageAsync(
                post.UserId,
                post.SelectedPackageId.Value);

            if (userCredits == null || userCredits.CreditsRemaining <= 0)
            {
                return BaseResponse<PostDetailResponse>.FailureResponse(
                    "Người bán không đủ credits cho gói tin này. Không thể duyệt bài đăng.");
            }

            // 4. Update post status to APPROVED
            post.Status = "APPROVED";
            post.ApprovedAt = DateTime.UtcNow;
            post.ApprovedBy = adminId;
            post.RejectedAt = null;
            post.RejectedBy = null;
            post.RejectionReason = null;

            await _postRepository.UpdateAsync(post);

            // 5. Deduct credits and create PostSubscription
            userCredits.CreditsRemaining -= 1;
            await _userPackageCreditsRepository.UpdateAsync(userCredits);

            var subscription = new PostSubscription
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserCreditId = userCredits.UserCreditId,
                PackageId = post.SelectedPackageId.Value,
                CreditsUsed = 1,
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _postSubscriptionRepository.CreateAsync(subscription);

            // 6. Save all changes
            await _postRepository.SaveChangesAsync();
            await _postSubscriptionRepository.SaveChangesAsync();
            await _userPackageCreditsRepository.SaveChangesAsync();

            // 7. Create notification for seller
            await _notificationService.CreateNotificationAsync(
                post.UserId,
                "POST_APPROVED",
                "Bài đăng đã được duyệt",
                $"Bài đăng \"{post.Title}\" đã được Admin duyệt và đang hiển thị công khai.",
                postId);

            _logger.LogInformation(
                "Admin {AdminId} đã duyệt bài đăng {PostId}, trừ {Credits} credit từ gói {PackageId}",
                adminId,
                postId,
                1,
                post.SelectedPackageId.Value);

            // 8. Get updated post with details
            var updatedPost = await _postRepository.GetByIdWithDetailsAsync(postId);
            
            // Load admin info if needed
            var admins = new Dictionary<Guid, Domain.Entities.User>();
            if (updatedPost!.ApprovedBy.HasValue)
            {
                var approvedByAdmin = await _userRepository.GetByIdAsync(updatedPost.ApprovedBy.Value);
                if (approvedByAdmin != null) admins[approvedByAdmin.Id] = approvedByAdmin;
            }
            if (updatedPost.RejectedBy.HasValue)
            {
                var rejectedByAdmin = await _userRepository.GetByIdAsync(updatedPost.RejectedBy.Value);
                if (rejectedByAdmin != null) admins[rejectedByAdmin.Id] = rejectedByAdmin;
            }
            
            var response = MapToDetailResponse(updatedPost, admins);

            return BaseResponse<PostDetailResponse>.SuccessResponse(
                response,
                "Duyệt bài đăng thành công. Credits đã được trừ. Staff sẽ được gán khi có người yêu cầu tư vấn.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi duyệt bài đăng, PostId: {PostId}, AdminId: {AdminId}", postId, adminId);
            return BaseResponse<PostDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi duyệt bài đăng");
        }
    }

    public async Task<BaseResponse> RejectPostAsync(
        Guid adminId,
        Guid postId,
        PostRejectRequest request)
    {
        try
        {
            // 1. Validate post exists and is PENDING (use tracking for update)
            var post = await _postRepository.GetByIdForUpdateAsync(postId);
            if (post == null)
            {
                return BaseResponse.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            if (post.Status != "PENDING")
            {
                return BaseResponse.FailureResponse(
                    $"Bài đăng không ở trạng thái PENDING. Trạng thái hiện tại: {post.Status}");
            }

            // 2. Update post status to DENIED
            post.Status = "DENIED";
            post.RejectedAt = DateTime.UtcNow;
            post.RejectedBy = adminId;
            post.RejectionReason = request.RejectionReason;
            post.ApprovedAt = null;
            post.ApprovedBy = null;

            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangesAsync();

            // 3. Create notification for seller
            await _notificationService.CreateNotificationAsync(
                post.UserId,
                "POST_REJECTED",
                "Bài đăng bị từ chối",
                $"Bài đăng \"{post.Title}\" đã bị từ chối. Lý do: {request.RejectionReason}",
                postId);

            _logger.LogInformation(
                "Admin {AdminId} đã từ chối bài đăng {PostId}, Lý do: {Reason}",
                adminId,
                postId,
                request.RejectionReason);

            return BaseResponse.SuccessResponse(
                "Từ chối bài đăng thành công. Người bán đã nhận được thông báo.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi từ chối bài đăng, PostId: {PostId}, AdminId: {AdminId}", postId, adminId);
            return BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi từ chối bài đăng");
        }
    }

    private PostResponse MapToResponse(Post post, Category category)
    {
        var imageUrls = post.PostImages
            .Where(img => !img.IsProof)
            .OrderBy(img => img.DisplayOrder)
            .Select(img => img.ImageUrl)
            .ToList();

        var proofImageUrl = post.PostImages
            .FirstOrDefault(img => img.IsProof)?.ImageUrl;

        // Get PriorityLevel from PostSubscription (latest applied subscription)
        int? priorityLevel = post.PostSubscriptions
            .OrderByDescending(ps => ps.AppliedAt)
            .FirstOrDefault()?.Package?.PriorityLevel;

        return new PostResponse
        {
            PostId = post.Id,
            UserId = post.UserId,
            CategoryId = post.CategoryId,
            CategoryName = category.Name,
            Title = post.Title,
            Description = post.Description,
            Price = post.Price,
            Location = post.Location,
            Brand = post.Brand,
            Model = post.Model,
            BatteryCapacityCurrent = post.BatteryCapacityCurrent,
            ChargeCount = post.ChargeCount,
            ProductionYear = post.ProductionYear,
            Condition = post.Condition,
            Mileage = post.Mileage,
            Status = post.Status,
            IsActive = post.IsActive,
            IsSold = post.IsSold,
            CreatedAt = post.CreatedAt,
            ImageUrls = imageUrls,
            ProofImageUrl = proofImageUrl,
            AuctionEnabled = post.AuctionEnabled,
            StartingBid = post.StartingBid,
            BuyNowPrice = post.BuyNowPrice,
            AuctionEndTime = post.AuctionEndTime,
            PriorityLevel = priorityLevel
        };
    }

    private PostDetailResponse MapToDetailResponse(Post post, Dictionary<Guid, Domain.Entities.User>? admins = null)
    {
        var baseResponse = MapToResponse(post, post.Category);
        var detailResponse = new PostDetailResponse
        {
            PostId = baseResponse.PostId,
            UserId = baseResponse.UserId,
            CategoryId = baseResponse.CategoryId,
            CategoryName = baseResponse.CategoryName,
            Title = baseResponse.Title,
            Description = baseResponse.Description,
            Price = baseResponse.Price,
            Location = baseResponse.Location,
            Brand = baseResponse.Brand,
            Model = baseResponse.Model,
            BatteryCapacityCurrent = baseResponse.BatteryCapacityCurrent,
            ChargeCount = baseResponse.ChargeCount,
            ProductionYear = baseResponse.ProductionYear,
            Condition = baseResponse.Condition,
            Mileage = baseResponse.Mileage,
            Status = baseResponse.Status,
            IsActive = baseResponse.IsActive,
            IsSold = baseResponse.IsSold,
            CreatedAt = baseResponse.CreatedAt,
            ImageUrls = baseResponse.ImageUrls,
            ProofImageUrl = baseResponse.ProofImageUrl,
            AuctionEnabled = baseResponse.AuctionEnabled,
            StartingBid = baseResponse.StartingBid,
            BuyNowPrice = baseResponse.BuyNowPrice,
            AuctionEndTime = baseResponse.AuctionEndTime,
            RejectionReason = post.RejectionReason,
            ApprovedAt = post.ApprovedAt,
            RejectedAt = post.RejectedAt
        };

        // Map staff assignment
        var activeAssignment = post.PostStaffAssignments.FirstOrDefault(psa => psa.IsActive);
        if (activeAssignment != null)
        {
            detailResponse.StaffAssignment = new StaffAssignmentInfo
            {
                StaffId = activeAssignment.StaffId,
                StaffName = activeAssignment.Staff.FullName ?? activeAssignment.Staff.Email,
                StaffEmail = activeAssignment.Staff.Email,
                AssignedAt = activeAssignment.AssignedAt
            };
        }

        // Map subscription
        var subscription = post.PostSubscriptions.FirstOrDefault();
        if (subscription != null && subscription.Package != null)
        {
            detailResponse.Subscription = new SubscriptionInfo
            {
                PackageId = subscription.PackageId,
                PackageName = subscription.Package.Name,
                PriorityLevel = subscription.Package.PriorityLevel,
                AppliedAt = subscription.AppliedAt
            };
        }

        // Map seller info
        if (post.User != null)
        {
            detailResponse.Seller = new SellerInfo
            {
                UserId = post.User.Id,
                FullName = post.User.FullName ?? string.Empty,
                Email = post.User.Email,
                PhoneNumber = post.User.PhoneNumber
            };
        }

        // Map admin info
        if (post.ApprovedBy.HasValue && admins != null && admins.TryGetValue(post.ApprovedBy.Value, out var approvedByAdmin))
        {
            detailResponse.ApprovedBy = new AdminInfo
            {
                AdminId = approvedByAdmin.Id,
                FullName = approvedByAdmin.FullName ?? approvedByAdmin.Email,
                Email = approvedByAdmin.Email
            };
        }

        if (post.RejectedBy.HasValue && admins != null && admins.TryGetValue(post.RejectedBy.Value, out var rejectedByAdmin))
        {
            detailResponse.RejectedBy = new AdminInfo
            {
                AdminId = rejectedByAdmin.Id,
                FullName = rejectedByAdmin.FullName ?? rejectedByAdmin.Email,
                Email = rejectedByAdmin.Email
            };
        }

        return detailResponse;
    }

    private PostPublicDetailResponse MapToPublicDetailResponse(Post post)
    {
        var imageUrls = post.PostImages
            .Where(img => !img.IsProof)
            .OrderBy(img => img.DisplayOrder)
            .Select(img => img.ImageUrl)
            .ToList();

        var proofImageUrl = post.PostImages
            .FirstOrDefault(img => img.IsProof)?.ImageUrl;

        // Chỉ hiển thị tên người bán, không hiển thị email/phone
        var sellerName = post.User?.FullName ?? post.User?.Email ?? "Người bán";

        // Get PriorityLevel from PostSubscription (latest applied subscription)
        int? priorityLevel = post.PostSubscriptions
            .OrderByDescending(ps => ps.AppliedAt)
            .FirstOrDefault()?.Package?.PriorityLevel;

        return new PostPublicDetailResponse
        {
            PostId = post.Id,
            UserId = post.UserId,
            CategoryId = post.CategoryId,
            CategoryName = post.Category?.Name ?? string.Empty,
            Title = post.Title,
            Description = post.Description,
            Price = post.Price,
            Location = post.Location,
            Brand = post.Brand,
            Model = post.Model,
            BatteryCapacityCurrent = post.BatteryCapacityCurrent,
            ChargeCount = post.ChargeCount,
            ProductionYear = post.ProductionYear,
            Condition = post.Condition,
            Mileage = post.Mileage,
            Status = post.Status,
            IsActive = post.IsActive,
            IsSold = post.IsSold,
            CreatedAt = post.CreatedAt,
            ImageUrls = imageUrls,
            ProofImageUrl = proofImageUrl,
            AuctionEnabled = post.AuctionEnabled,
            StartingBid = post.StartingBid,
            BuyNowPrice = post.BuyNowPrice,
            AuctionEndTime = post.AuctionEndTime,
            SellerName = sellerName,
            PriorityLevel = priorityLevel
        };
    }

    private PendingPostResponse MapToPendingResponse(Post post)
    {
        var thumbnailUrl = post.PostImages
            .FirstOrDefault(img => img.IsThumbnail)?.ImageUrl;

        var hasProofImage = post.PostImages.Any(img => img.IsProof);

        return new PendingPostResponse
        {
            PostId = post.Id,
            Title = post.Title,
            Price = post.Price,
            Brand = post.Brand,
            Model = post.Model,
            CategoryName = post.Category?.Name ?? string.Empty,
            SellerName = post.User?.FullName ?? post.User?.Email ?? string.Empty,
            SellerEmail = post.User?.Email ?? string.Empty,
            CreatedAt = post.CreatedAt,
            ThumbnailUrl = thumbnailUrl,
            HasProofImage = hasProofImage
        };
    }

    private ApprovedRejectedPostResponse MapToApprovedRejectedResponse(
        Post post, 
        Dictionary<Guid, Domain.Entities.User>? admins = null)
    {
        var thumbnailUrl = post.PostImages
            .FirstOrDefault(img => img.IsThumbnail)?.ImageUrl;

        var hasProofImage = post.PostImages.Any(img => img.IsProof);

        // Get assigned staff name if approved
        string? assignedStaffName = null;
        if (post.Status == "APPROVED")
        {
            var activeAssignment = post.PostStaffAssignments.FirstOrDefault(psa => psa.IsActive);
            if (activeAssignment != null && activeAssignment.Staff != null)
            {
                assignedStaffName = activeAssignment.Staff.FullName ?? activeAssignment.Staff.Email;
            }
        }

        // Get admin info
        string? approvedByName = null;
        string? approvedByEmail = null;
        if (post.ApprovedBy.HasValue && admins != null && admins.TryGetValue(post.ApprovedBy.Value, out var approvedByAdmin))
        {
            approvedByName = approvedByAdmin.FullName ?? approvedByAdmin.Email;
            approvedByEmail = approvedByAdmin.Email;
        }

        string? rejectedByName = null;
        string? rejectedByEmail = null;
        if (post.RejectedBy.HasValue && admins != null && admins.TryGetValue(post.RejectedBy.Value, out var rejectedByAdmin))
        {
            rejectedByName = rejectedByAdmin.FullName ?? rejectedByAdmin.Email;
            rejectedByEmail = rejectedByAdmin.Email;
        }

        return new ApprovedRejectedPostResponse
        {
            PostId = post.Id,
            Title = post.Title,
            Price = post.Price,
            Brand = post.Brand,
            Model = post.Model,
            CategoryName = post.Category?.Name ?? string.Empty,
            SellerName = post.User?.FullName ?? post.User?.Email ?? string.Empty,
            SellerEmail = post.User?.Email ?? string.Empty,
            Status = post.Status,
            CreatedAt = post.CreatedAt,
            ApprovedAt = post.ApprovedAt,
            RejectedAt = post.RejectedAt,
            RejectionReason = post.RejectionReason,
            AssignedStaffName = assignedStaffName,
            ApprovedByName = approvedByName,
            ApprovedByEmail = approvedByEmail,
            RejectedByName = rejectedByName,
            RejectedByEmail = rejectedByEmail,
            ThumbnailUrl = thumbnailUrl,
            HasProofImage = hasProofImage
        };
    }

    public async Task<BaseResponse<PostResponse>> UpdatePostAsync(
        Guid userId,
        Guid postId,
        PostUpdateRequest request)
    {
        try
        {
            // 1. Get post with images (with tracking for update)
            var post = await _postRepository.GetByIdForUpdateWithImagesAsync(postId);
            if (post == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 2. Validate ownership
            if (post.UserId != userId)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Bạn không có quyền chỉnh sửa bài đăng này");
            }

            // 3. Check if post is sold
            if (post.IsSold)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Không thể chỉnh sửa bài đăng đã được bán");
            }

            // 4. Update basic fields (only if provided)
            if (request.Title != null)
                post.Title = request.Title;
            if (request.Description != null)
                post.Description = request.Description;
            if (request.Price.HasValue)
                post.Price = request.Price.Value;
            if (request.Location != null)
                post.Location = request.Location;
            if (request.Brand != null)
                post.Brand = request.Brand;
            if (request.Model != null)
                post.Model = request.Model;
            if (request.BatteryCapacityCurrent.HasValue)
                post.BatteryCapacityCurrent = request.BatteryCapacityCurrent.Value;
            if (request.ChargeCount.HasValue)
                post.ChargeCount = request.ChargeCount.Value;
            if (request.ProductionYear.HasValue)
                post.ProductionYear = request.ProductionYear.Value;
            if (request.Condition != null)
                post.Condition = request.Condition;
            if (request.Mileage.HasValue)
                post.Mileage = request.Mileage.Value;

            // 5. Update auction fields
            if (request.AuctionEnabled.HasValue)
            {
                post.AuctionEnabled = request.AuctionEnabled.Value;
                if (!request.AuctionEnabled.Value)
                {
                    // Nếu tắt đấu giá, xóa các thông tin đấu giá
                    post.StartingBid = null;
                    post.BuyNowPrice = null;
                    post.AuctionEndTime = null;
                }
            }
            if (request.StartingBid.HasValue)
                post.StartingBid = request.StartingBid.Value;
            if (request.BuyNowPrice.HasValue)
                post.BuyNowPrice = request.BuyNowPrice.Value;
            if (request.AuctionEndTime.HasValue)
                post.AuctionEndTime = request.AuctionEndTime.Value;

            // 6. Handle image deletion
            if (request.ImagesToDelete != null && request.ImagesToDelete.Any())
            {
                var imagesToDelete = post.PostImages
                    .Where(img => request.ImagesToDelete.Contains(img.ImageUrl))
                    .ToList();

                foreach (var image in imagesToDelete)
                {
                    // Delete file from storage
                    await _fileStorageService.DeleteImageAsync(image.ImageUrl);
                    // Remove from collection (EF Core will delete from DB)
                    post.PostImages.Remove(image);
                }
            }

            // 7. Handle new images
            if (request.NewImages != null && request.NewImages.Any())
            {
                // Validate package max images if post has package
                if (post.SelectedPackageId.HasValue)
                {
                    var package = await _packageRepository.GetByIdAsync(post.SelectedPackageId.Value);
                    if (package != null)
                    {
                        var totalImagesAfterUpdate = post.PostImages.Count(img => !img.IsProof) + request.NewImages.Count;
                        if (totalImagesAfterUpdate > package.MaxImages)
                        {
                            return BaseResponse<PostResponse>.FailureResponse(
                                $"Gói tin {package.Name} chỉ cho phép tối đa {package.MaxImages} ảnh. Tổng số ảnh sau khi cập nhật sẽ là {totalImagesAfterUpdate}.");
                        }
                    }
                }

                // Upload new images
                var currentImageCount = post.PostImages.Count(img => !img.IsProof);
                for (int i = 0; i < request.NewImages.Count; i++)
                {
                    var imageUrl = await _fileStorageService.UploadImageAsync(request.NewImages[i]);
                    post.PostImages.Add(new PostImage
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        ImageUrl = imageUrl,
                        IsThumbnail = currentImageCount == 0 && i == 0, // First image is thumbnail if no existing images
                        IsProof = false,
                        DisplayOrder = currentImageCount + i,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Update thumbnail if needed (first image should be thumbnail)
                var existingThumbnail = post.PostImages.FirstOrDefault(img => img.IsThumbnail && !img.IsProof);
                if (existingThumbnail == null && post.PostImages.Any(img => !img.IsProof))
                {
                    var firstImage = post.PostImages
                        .Where(img => !img.IsProof)
                        .OrderBy(img => img.DisplayOrder)
                        .First();
                    firstImage.IsThumbnail = true;
                }
            }

            // 8. Handle new proof image
            if (request.NewProofImage != null)
            {
                // Delete old proof image
                var oldProofImage = post.PostImages.FirstOrDefault(img => img.IsProof);
                if (oldProofImage != null)
                {
                    await _fileStorageService.DeleteImageAsync(oldProofImage.ImageUrl);
                    post.PostImages.Remove(oldProofImage);
                }

                // Upload new proof image
                var proofImageUrl = await _fileStorageService.UploadImageAsync(request.NewProofImage);
                post.PostImages.Add(new PostImage
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ImageUrl = proofImageUrl,
                    IsThumbnail = false,
                    IsProof = true,
                    DisplayOrder = post.PostImages.Count,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 9. If post was APPROVED, change status to PENDING for re-approval (UC07)
            var statusChanged = false;
            if (post.Status == "APPROVED")
            {
                post.Status = "PENDING";
                post.ApprovedAt = null;
                post.ApprovedBy = null;
                statusChanged = true;

                // Thông báo cho seller về việc bài đăng cần duyệt lại
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        post.UserId,
                        "POST_NEEDS_REAPPROVAL",
                        "Bài đăng cần được duyệt lại",
                        $"Bài đăng \"{post.Title}\" đã được chỉnh sửa và cần được Admin duyệt lại trước khi hiển thị công khai.",
                        postId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi gửi thông báo cho seller về bài đăng cần duyệt lại, PostId: {PostId}", postId);
                }

                // Thông báo cho tất cả Admin về bài đăng cần duyệt lại
                try
                {
                    var admins = await _userRepository.GetAdminsAsync();
                    var seller = await _userRepository.GetByIdAsync(post.UserId);
                    var sellerName = seller?.FullName ?? seller?.Email ?? "Người dùng";
                    
                    foreach (var admin in admins)
                    {
                        await _notificationService.CreateNotificationAsync(
                            admin.Id,
                            "POST_NEEDS_REAPPROVAL",
                            "Có bài đăng cần duyệt lại",
                            $"Bài đăng \"{post.Title}\" của {sellerName} đã được chỉnh sửa và cần được duyệt lại.",
                            postId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi gửi thông báo cho admin về bài đăng cần duyệt lại, PostId: {PostId}", postId);
                }
            }

            // 10. Update post
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Cập nhật bài đăng thành công, PostId: {PostId}, UserId: {UserId}, StatusChanged: {StatusChanged}",
                postId,
                userId,
                statusChanged);

            // 11. Map to response
            var updatedPost = await _postRepository.GetByIdWithImagesAsync(postId);
            if (updatedPost == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Lỗi khi lấy thông tin bài đăng sau khi cập nhật");
            }

            var response = MapToResponse(updatedPost, updatedPost.Category);

            var message = statusChanged
                ? "Cập nhật bài đăng thành công. Bài đăng đã chuyển về trạng thái PENDING và cần được Admin duyệt lại."
                : "Cập nhật bài đăng thành công.";

            return BaseResponse<PostResponse>.SuccessResponse(
                response,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài đăng, PostId: {PostId}, UserId: {UserId}", postId, userId);
            return BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật bài đăng");
        }
    }

    public async Task<BaseResponse> DeletePostAsync(Guid userId, Guid postId)
    {
        try
        {
            // 1. Get post (with tracking for delete)
            var post = await _postRepository.GetByIdForUpdateWithImagesAsync(postId);
            if (post == null)
            {
                return BaseResponse.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 2. Validate ownership
            if (post.UserId != userId)
            {
                return BaseResponse.FailureResponse(
                    "Bạn không có quyền xóa bài đăng này");
            }

            // 3. Check if post is sold
            if (post.IsSold)
            {
                return BaseResponse.FailureResponse(
                    "Không thể xóa bài đăng đã được bán");
            }

            // 4. Delete images from storage
            foreach (var image in post.PostImages)
            {
                await _fileStorageService.DeleteImageAsync(image.ImageUrl);
            }

            // 5. Delete post (EF Core will cascade delete PostImages)
            await _postRepository.DeleteAsync(post);
            await _postRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Xóa bài đăng thành công, PostId: {PostId}, UserId: {UserId}",
                postId,
                userId);

            return BaseResponse.SuccessResponse(
                "Xóa bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài đăng, PostId: {PostId}, UserId: {UserId}", postId, userId);
            return BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi xóa bài đăng");
        }
    }

    public async Task<BaseResponse<PostResponse>> TogglePostActiveAsync(
        Guid userId,
        Guid postId,
        PostToggleActiveRequest request)
    {
        try
        {
            // 1. Get post (with tracking for update)
            var post = await _postRepository.GetByIdForUpdateAsync(postId);
            if (post == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 2. Validate ownership
            if (post.UserId != userId)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Bạn không có quyền thay đổi trạng thái bài đăng này");
            }

            // 3. Update IsActive
            post.IsActive = request.IsActive;

            // 4. Update post
            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Thay đổi trạng thái bài đăng thành công, PostId: {PostId}, UserId: {UserId}, IsActive: {IsActive}",
                postId,
                userId,
                request.IsActive);

            // 5. Map to response
            var updatedPost = await _postRepository.GetByIdWithImagesAsync(postId);
            if (updatedPost == null)
            {
                return BaseResponse<PostResponse>.FailureResponse(
                    "Lỗi khi lấy thông tin bài đăng sau khi cập nhật");
            }

            var response = MapToResponse(updatedPost, updatedPost.Category);

            var message = request.IsActive
                ? "Bài đăng đã được hiển thị"
                : "Bài đăng đã được ẩn";

            return BaseResponse<PostResponse>.SuccessResponse(
                response,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái bài đăng, PostId: {PostId}, UserId: {UserId}", postId, userId);
            return BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi thay đổi trạng thái bài đăng");
        }
    }

    public async Task<PagedResponse<PostResponse>> GetMyPostsAsync(
        Guid userId,
        MyPostsSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _postRepository.GetPostsByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                request.Keyword,
                request.CategoryId,
                request.Status,
                request.SortBy,
                request.SortDirection);

            var responses = result.Items.Select(post => MapToResponse(post, post.Category)).ToList();

            _logger.LogInformation(
                "User {UserId} lấy danh sách bài đăng của mình, Total: {TotalCount}, Status: {Status}",
                userId,
                result.TotalCount,
                request.Status ?? "ALL");

            return PagedResponse<PostResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách bài đăng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng của user, UserId: {UserId}", userId);
            return PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng");
        }
    }

    public async Task<BaseResponse<PostCompareResponse>> ComparePostsAsync(PostCompareRequest request)
    {
        try
        {
            // 1. Lấy posts từ repository (chỉ lấy posts đã được duyệt, đang hoạt động và chưa bán)
            var posts = await _postRepository.GetApprovedPostsByIdsAsync(request.PostIds);

            // 2. Kiểm tra số lượng posts hợp lệ
            if (posts.Count == 0)
            {
                return BaseResponse<PostCompareResponse>.FailureResponse(
                    "Không tìm thấy sản phẩm hợp lệ để so sánh. Vui lòng kiểm tra lại danh sách sản phẩm.");
            }

            // 3. Kiểm tra xem có post nào không tồn tại hoặc không hợp lệ không
            var foundPostIds = posts.Select(p => p.Id).ToList();
            var missingPostIds = request.PostIds.Except(foundPostIds).ToList();
            if (missingPostIds.Any())
            {
                _logger.LogWarning(
                    "Một số sản phẩm không tồn tại hoặc không hợp lệ để so sánh: {MissingPostIds}",
                    string.Join(", ", missingPostIds));
                
                // Vẫn trả về các posts hợp lệ, nhưng cảnh báo
                if (posts.Count < 2)
                {
                    return BaseResponse<PostCompareResponse>.FailureResponse(
                        "Không đủ sản phẩm hợp lệ để so sánh. Phải có ít nhất 2 sản phẩm đã được duyệt và đang bán.");
                }
            }

            // 4. Map posts sang PostCompareItem
            var compareItems = posts.Select(post => MapToCompareItem(post)).ToList();

            // 5. Sắp xếp theo thứ tự PostIds trong request để giữ nguyên thứ tự người dùng chọn
            var orderedItems = request.PostIds
                .Join(compareItems, id => id, item => item.PostId, (id, item) => item)
                .ToList();

            var response = new PostCompareResponse
            {
                Products = orderedItems
            };

            _logger.LogInformation(
                "So sánh {Count} sản phẩm thành công",
                orderedItems.Count);

            return BaseResponse<PostCompareResponse>.SuccessResponse(
                response,
                "So sánh sản phẩm thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi so sánh sản phẩm");
            return BaseResponse<PostCompareResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi so sánh sản phẩm");
        }
    }

    private PostCompareItem MapToCompareItem(Post post)
    {
        var thumbnailImageUrl = post.PostImages
            .Where(img => !img.IsProof)
            .OrderBy(img => img.DisplayOrder)
            .FirstOrDefault()?.ImageUrl;

        return new PostCompareItem
        {
            PostId = post.Id,
            Title = post.Title,
            ThumbnailImageUrl = thumbnailImageUrl,
            Price = post.Price,
            BatteryCapacityCurrent = post.BatteryCapacityCurrent,
            Mileage = post.Mileage,
            ProductionYear = post.ProductionYear,
            Brand = post.Brand,
            Model = post.Model,
            Condition = post.Condition,
            ChargeCount = post.ChargeCount,
            Location = post.Location,
            CategoryName = post.Category?.Name ?? string.Empty,
            AuctionEnabled = post.AuctionEnabled,
            StartingBid = post.StartingBid,
            BuyNowPrice = post.BuyNowPrice
        };
    }
}

