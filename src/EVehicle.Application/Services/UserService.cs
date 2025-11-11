using System.IO;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Users;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// User Service implementation (UC47)
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPostRepository postRepository,
        IOrderRepository orderRepository,
        ILeadRepository leadRepository,
        IPaymentRepository paymentRepository,
        IFileStorageService fileStorageService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _orderRepository = orderRepository;
        _leadRepository = leadRepository;
        _paymentRepository = paymentRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// UC47.1: Lấy danh sách người dùng với phân trang và lọc
    /// </summary>
    public async Task<PagedResponse<UserResponse>> GetUsersAsync(UserSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _userRepository.GetUsersAsync(
                request.Keyword,
                request.Role,
                request.Status,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection);

            var responses = result.Items.Select(MapToResponse).ToList();

            return PagedResponse<UserResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách người dùng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
            return PagedResponse<UserResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách người dùng");
        }
    }

    /// <summary>
    /// UC47.4: Lấy chi tiết thông tin người dùng
    /// </summary>
    public async Task<BaseResponse<UserResponse>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<UserResponse>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            var response = MapToResponseWithStats(user);

            return BaseResponse<UserResponse>.SuccessResponse(
                response,
                "Lấy thông tin người dùng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng {UserId}", userId);
            return BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin người dùng");
        }
    }

    /// <summary>
    /// UC47.2: Vô hiệu hóa/kích hoạt tài khoản
    /// UC47.3: Thay đổi role của người dùng
    /// </summary>
    public async Task<BaseResponse<UserResponse>> UpdateUserAsync(
        Guid userId,
        UserUpdateRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<UserResponse>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            // Update role nếu có
            if (!string.IsNullOrEmpty(request.Role))
            {
                if (request.Role != "MEMBER" && request.Role != "STAFF" && request.Role != "ADMIN")
                {
                    return BaseResponse<UserResponse>.FailureResponse(
                        "Role không hợp lệ. Role phải là MEMBER, STAFF hoặc ADMIN");
                }

                user.Role = request.Role;
                _logger.LogInformation("Cập nhật role của user {UserId} thành {Role}", userId, request.Role);
            }

            // Update status nếu có
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (request.Status != "ACTIVE" && request.Status != "BANNED" &&
                    request.Status != "SUSPENDED" && request.Status != "PENDING_VERIFICATION")
                {
                    return BaseResponse<UserResponse>.FailureResponse(
                        "Status không hợp lệ. Status phải là ACTIVE, BANNED, SUSPENDED hoặc PENDING_VERIFICATION");
                }

                user.Status = request.Status;
                _logger.LogInformation("Cập nhật status của user {UserId} thành {Status}", userId, request.Status);
            }

            // Lưu thay đổi
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            var response = MapToResponseWithStats(user);

            return BaseResponse<UserResponse>.SuccessResponse(
                response,
                "Cập nhật thông tin người dùng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng {UserId}", userId);
            return BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật thông tin người dùng");
        }
    }

    /// <summary>
    /// UC47.5: Lấy lịch sử hoạt động của người dùng
    /// </summary>
    public async Task<BaseResponse<List<UserActivityResponse>>> GetUserActivityAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<List<UserActivityResponse>>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            var activities = new List<UserActivityResponse>();

            // 1. Lấy lịch sử Posts (tạo và cập nhật)
            try
            {
                var postsResult = await _postRepository.GetPostsByUserIdAsync(
                    userId,
                    pageNumber: 1,
                    pageSize: 100, // Lấy nhiều posts để có đầy đủ lịch sử
                    keyword: null,
                    categoryId: null,
                    status: null,
                    sortBy: "CreatedAt",
                    sortDirection: "DESC");

                foreach (var post in postsResult.Items)
                {
                    // Activity: Tạo bài đăng
                    activities.Add(new UserActivityResponse
                    {
                        ActivityId = Guid.NewGuid(),
                        ActivityType = "POST_CREATED",
                        Description = $"Đã tạo bài đăng: {post.Title}",
                        RelatedId = post.Id,
                        CreatedAt = post.CreatedAt
                    });

                    // Activity: Cập nhật bài đăng (nếu có)
                    if (post.UpdatedAt.HasValue && post.UpdatedAt.Value > post.CreatedAt)
                    {
                        activities.Add(new UserActivityResponse
                        {
                            ActivityId = Guid.NewGuid(),
                            ActivityType = "POST_UPDATED",
                            Description = $"Đã cập nhật bài đăng: {post.Title}",
                            RelatedId = post.Id,
                            CreatedAt = post.UpdatedAt.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy lịch sử Posts cho user {UserId}", userId);
            }

            // 2. Lấy lịch sử Orders (như Buyer)
            try
            {
                var buyerOrders = await _orderRepository.GetOrdersByBuyerIdAsync(userId);
                foreach (var order in buyerOrders)
                {
                    // Lấy thông tin Post để có description tốt hơn
                    var post = await _postRepository.GetByIdAsync(order.PostId);
                    var postTitle = post != null ? post.Title : "Sản phẩm";
                    var orderShortId = order.Id.ToString().Substring(0, 8).ToUpper();
                    
                    activities.Add(new UserActivityResponse
                    {
                        ActivityId = Guid.NewGuid(),
                        ActivityType = "ORDER_CREATED",
                        Description = $"Đã tạo đơn hàng #{orderShortId}: {postTitle} - {order.FinalPrice:N0} VNĐ",
                        RelatedId = order.Id,
                        CreatedAt = order.CreatedAt
                    });

                    if (order.UpdatedAt.HasValue && order.UpdatedAt.Value > order.CreatedAt)
                    {
                        activities.Add(new UserActivityResponse
                        {
                            ActivityId = Guid.NewGuid(),
                            ActivityType = "ORDER_UPDATED",
                            Description = $"Đã cập nhật đơn hàng #{orderShortId}: {postTitle}",
                            RelatedId = order.Id,
                            CreatedAt = order.UpdatedAt.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy lịch sử Orders (Buyer) cho user {UserId}", userId);
            }

            // 3. Lấy lịch sử Orders (như Seller)
            try
            {
                var sellerOrders = await _orderRepository.GetOrdersBySellerIdAsync(userId);
                foreach (var order in sellerOrders)
                {
                    // Lấy thông tin Post để có description tốt hơn
                    var post = await _postRepository.GetByIdAsync(order.PostId);
                    var postTitle = post != null ? post.Title : "Sản phẩm";
                    var orderShortId = order.Id.ToString().Substring(0, 8).ToUpper();
                    
                    activities.Add(new UserActivityResponse
                    {
                        ActivityId = Guid.NewGuid(),
                        ActivityType = "ORDER_RECEIVED",
                        Description = $"Đã nhận đơn hàng #{orderShortId}: {postTitle} - {order.FinalPrice:N0} VNĐ",
                        RelatedId = order.Id,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy lịch sử Orders (Seller) cho user {UserId}", userId);
            }

            // 4. Lấy lịch sử Leads (như Buyer)
            try
            {
                var leads = await _leadRepository.GetLeadsByBuyerIdAsync(userId);
                foreach (var lead in leads)
                {
                    // Lấy thông tin Post để có description tốt hơn
                    var post = await _postRepository.GetByIdAsync(lead.PostId);
                    var postTitle = post != null ? post.Title : "Bài đăng";
                    
                    var leadTypeDescription = lead.LeadType == "AUCTION_WINNER" 
                        ? "Đấu giá thắng" 
                        : "Đặt lịch xem";

                    activities.Add(new UserActivityResponse
                    {
                        ActivityId = Guid.NewGuid(),
                        ActivityType = "LEAD_CREATED",
                        Description = $"Đã tạo Lead ({leadTypeDescription}): {postTitle}",
                        RelatedId = lead.Id,
                        CreatedAt = lead.CreatedAt
                    });

                    if (lead.UpdatedAt.HasValue && lead.UpdatedAt.Value > lead.CreatedAt)
                    {
                        activities.Add(new UserActivityResponse
                        {
                            ActivityId = Guid.NewGuid(),
                            ActivityType = "LEAD_UPDATED",
                            Description = $"Đã cập nhật Lead: {postTitle}",
                            RelatedId = lead.Id,
                            CreatedAt = lead.UpdatedAt.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy lịch sử Leads cho user {UserId}", userId);
            }

            // 5. Lấy lịch sử Payments
            try
            {
                var payments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
                foreach (var payment in payments)
                {
                    var paymentTypeDescription = payment.PaymentType == "PACKAGE" 
                        ? "Thanh toán gói tin" 
                        : "Thanh toán đơn hàng";

                    activities.Add(new UserActivityResponse
                    {
                        ActivityId = Guid.NewGuid(),
                        ActivityType = "PAYMENT_CREATED",
                        Description = $"{paymentTypeDescription}: {payment.Amount:N0} VNĐ",
                        RelatedId = payment.Id,
                        CreatedAt = payment.CreatedAt
                    });

                    if (payment.UpdatedAt.HasValue && payment.UpdatedAt.Value > payment.CreatedAt)
                    {
                        activities.Add(new UserActivityResponse
                        {
                            ActivityId = Guid.NewGuid(),
                            ActivityType = "PAYMENT_UPDATED",
                            Description = $"Đã cập nhật {paymentTypeDescription}",
                            RelatedId = payment.Id,
                            CreatedAt = payment.UpdatedAt.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy lịch sử Payments cho user {UserId}", userId);
            }

            // Sắp xếp theo thời gian (mới nhất trước)
            activities = activities
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            _logger.LogInformation(
                "Đã lấy {Count} hoạt động cho user {UserId}",
                activities.Count,
                userId);

            return BaseResponse<List<UserActivityResponse>>.SuccessResponse(
                activities,
                "Lấy lịch sử hoạt động thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử hoạt động của người dùng {UserId}", userId);
            return BaseResponse<List<UserActivityResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy lịch sử hoạt động");
        }
    }

    /// <summary>
    /// Map User entity sang UserResponse (không có stats)
    /// </summary>
    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            IdNumber = user.IdNumber,
            IdFrontImageUrl = user.IdFrontImageUrl,
            IdBackImageUrl = user.IdBackImageUrl,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            EmailVerifiedAt = user.EmailVerifiedAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Map User entity sang UserResponse (có stats)
    /// </summary>
    private UserResponse MapToResponseWithStats(User user)
    {
        var response = MapToResponse(user);

        // TODO: Tính toán stats từ các bảng liên quan
        // Có thể inject thêm repositories để lấy stats:
        // - TotalPosts: đếm từ Posts
        // - ActivePosts: đếm từ Posts với is_active = true
        // - TotalLeads: đếm từ Leads
        // - TotalOrders: đếm từ Orders
        //
        // Hiện tại để giá trị mặc định là 0

        return response;
    }

    /// <summary>
    /// UC04: Member tự cập nhật thông tin profile của mình
    /// </summary>
    public async Task<BaseResponse<UserResponse>> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<UserResponse>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            // Update FullName nếu có
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
                _logger.LogInformation("Cập nhật FullName của user {UserId}", userId);
            }

            // Update Address nếu có
            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                user.Address = request.Address.Trim();
                _logger.LogInformation("Cập nhật Address của user {UserId}", userId);
            }

            // Update AvatarUrl nếu có
            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            {
                // Xóa avatar cũ nếu có
                if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                {
                    try
                    {
                        await _fileStorageService.DeleteImageAsync(user.AvatarUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể xóa avatar cũ của user {UserId}", userId);
                    }
                }

                user.AvatarUrl = request.AvatarUrl.Trim();
                _logger.LogInformation("Cập nhật AvatarUrl của user {UserId}", userId);
            }

            // Update IdNumber nếu có
            if (!string.IsNullOrWhiteSpace(request.IdNumber))
            {
                user.IdNumber = request.IdNumber.Trim();
                _logger.LogInformation("Cập nhật IdNumber của user {UserId}", userId);
            }

            // Lưu thay đổi
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            var response = MapToResponseWithStats(user);

            return BaseResponse<UserResponse>.SuccessResponse(
                response,
                "Cập nhật thông tin profile thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật profile của user {UserId}", userId);
            return BaseResponse<UserResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật thông tin profile");
        }
    }

    /// <summary>
    /// UC04: Upload avatar cho user hiện tại
    /// </summary>
    public async Task<BaseResponse<string>> UploadAvatarAsync(
        Guid userId,
        DTOs.Common.FileUploadDto file)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<string>.FailureResponse(
                    "Không tìm thấy người dùng");
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BaseResponse<string>.FailureResponse(
                    "File không hợp lệ");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BaseResponse<string>.FailureResponse(
                    "Định dạng file không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG");
            }

            // Validate file size (max 5MB)
            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
            {
                return BaseResponse<string>.FailureResponse(
                    "Kích thước file không được vượt quá 5MB");
            }

            // Validate content type
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BaseResponse<string>.FailureResponse(
                    "Content type không hợp lệ");
            }

            // Xóa avatar cũ nếu có
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                try
                {
                    await _fileStorageService.DeleteImageAsync(user.AvatarUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể xóa avatar cũ của user {UserId}", userId);
                }
            }

            // Upload avatar mới
            var avatarUrl = await _fileStorageService.UploadImageAsync(file);

            // Cập nhật AvatarUrl cho user
            user.AvatarUrl = avatarUrl;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Upload avatar thành công cho user {UserId}: {AvatarUrl}", userId, avatarUrl);

            return BaseResponse<string>.SuccessResponse(
                avatarUrl,
                "Upload avatar thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload avatar cho user {UserId}", userId);
            return BaseResponse<string>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi upload avatar");
        }
    }
}

