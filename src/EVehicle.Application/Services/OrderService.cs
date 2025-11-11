using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Order Service implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserPackageCreditsRepository _userPackageCreditsRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        ILeadRepository leadRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IUserPackageCreditsRepository userPackageCreditsRepository,
        IPackageRepository packageRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _leadRepository = leadRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _userPackageCreditsRepository = userPackageCreditsRepository;
        _packageRepository = packageRepository;
        _logger = logger;
    }

    /// <summary>
    /// UC28: Staff tạo đơn hàng
    /// </summary>
    public async Task<BaseResponse<OrderResponse>> CreateOrderAsync(
        Guid staffId,
        OrderCreateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Validate Post exists (use GetByIdForUpdateAsync to get tracking entity)
            var post = await _postRepository.GetByIdForUpdateAsync(request.PostId);
            if (post == null)
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 3. Validate Post is not sold
            if (post.IsSold)
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Sản phẩm đã được bán");
            }

            // 4. Validate Buyer exists
            var buyer = await _userRepository.GetByIdAsync(request.BuyerId);
            if (buyer == null)
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Không tìm thấy người mua");
            }

            // 5. Validate Lead if provided
            Lead? lead = null;
            if (request.LeadId.HasValue)
            {
                lead = await _leadRepository.GetByIdWithDetailsAsync(request.LeadId.Value);
                if (lead == null)
                {
                    return BaseResponse<OrderResponse>.FailureResponse(
                        "Không tìm thấy Lead");
                }

                // Validate Lead belongs to this Staff
                if (lead.StaffId != staffId)
                {
                    return BaseResponse<OrderResponse>.FailureResponse(
                        "Bạn không có quyền tạo đơn hàng cho Lead này");
                }

                // Validate Lead status is SUCCESSFUL
                if (lead.Status != "SUCCESSFUL")
                {
                    return BaseResponse<OrderResponse>.FailureResponse(
                        "Lead phải ở trạng thái SUCCESSFUL để tạo đơn hàng");
                }
            }

            // 6. Check if Order already exists for this Lead
            if (request.LeadId.HasValue)
            {
                var existingOrder = await _orderRepository.GetByLeadIdAsync(request.LeadId.Value);
                if (existingOrder != null)
                {
                    return BaseResponse<OrderResponse>.FailureResponse(
                        "Đơn hàng đã tồn tại cho Lead này");
                }
            }

            // 7. Create Order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                LeadId = request.LeadId,
                PostId = request.PostId,
                BuyerId = request.BuyerId,
                SellerId = post.UserId,
                StaffId = staffId,
                FinalPrice = request.FinalPrice,
                Status = "PENDING_PAYMENT",
                PaymentMethod = null,
                ShippingAddress = request.ShippingAddress,
                PaidAt = null,
                CompletedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.CreateAsync(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã tạo đơn hàng {OrderId} cho Lead {LeadId}, Post {PostId}, Buyer {BuyerId}",
                staffId,
                order.Id,
                request.LeadId,
                request.PostId,
                request.BuyerId);

            // 8. Create notifications
            var buyerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.BuyerId,
                NotificationType = "ORDER_CREATED",
                Title = "Đơn hàng đã được tạo",
                Content = $"Đơn hàng cho bài đăng \"{post.Title}\" đã được tạo. Vui lòng thanh toán.",
                RelatedId = order.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(buyerNotification);

            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = post.UserId,
                NotificationType = "ORDER_CREATED",
                Title = "Có đơn hàng mới",
                Content = $"Có đơn hàng mới cho bài đăng \"{post.Title}\". Đang chờ thanh toán.",
                RelatedId = order.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(sellerNotification);

            await _notificationRepository.SaveChangesAsync();

            // 9. Get order with details for response
            var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
            if (orderWithDetails == null)
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Không tìm thấy đơn hàng vừa tạo");
            }

            // 10. Map to response
            var response = MapToResponse(orderWithDetails);

            return BaseResponse<OrderResponse>.SuccessResponse(
                response,
                "Tạo đơn hàng thành công. Người mua có thể thanh toán.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo đơn hàng, StaffId: {StaffId}", staffId);
            return BaseResponse<OrderResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo đơn hàng");
        }
    }

    /// <summary>
    /// Lấy chi tiết Order
    /// </summary>
    public async Task<BaseResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId, Guid? userId = null)
    {
        try
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                return BaseResponse<OrderResponse>.FailureResponse(
                    "Không tìm thấy đơn hàng");
            }

            // Validate user has permission to view this order
            if (userId.HasValue)
            {
                if (order.BuyerId != userId.Value && order.SellerId != userId.Value)
                {
                    // Check if user is Staff assigned to this order
                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    if (user == null || (user.Role != "STAFF" && user.Role != "ADMIN") || order.StaffId != userId.Value)
                    {
                        return BaseResponse<OrderResponse>.FailureResponse(
                            "Bạn không có quyền xem đơn hàng này");
                    }
                }
            }

            var response = MapToResponse(order);

            return BaseResponse<OrderResponse>.SuccessResponse(
                response,
                "Lấy chi tiết đơn hàng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng, OrderId: {OrderId}", orderId);
            return BaseResponse<OrderResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết đơn hàng");
        }
    }

    /// <summary>
    /// UC28: Tạo thanh toán cho đơn hàng
    /// </summary>
    public async Task<BaseResponse<PaymentResponse>> CreatePaymentAsync(
        Guid userId,
        PaymentCreateRequest request)
    {
        try
        {
            // 1. Validate Order exists
            var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Không tìm thấy đơn hàng");
            }

            // 2. Validate Order belongs to this user (buyer)
            if (order.BuyerId != userId)
            {
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Bạn không có quyền thanh toán đơn hàng này");
            }

            // 3. Validate Order status is PENDING_PAYMENT
            if (order.Status != "PENDING_PAYMENT")
            {
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Đơn hàng không ở trạng thái chờ thanh toán");
            }

            // 4. Check if Payment already exists
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
            if (existingPayment != null && existingPayment.Status == "PENDING")
            {
                // Return existing payment URL if available
                return BaseResponse<PaymentResponse>.SuccessResponse(
                    MapPaymentToResponse(existingPayment),
                    "Đã có thanh toán đang chờ xử lý");
            }

            // 5. Create Payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderId = request.OrderId,
                Amount = order.FinalPrice,
                PaymentGateway = request.PaymentGateway.ToUpper(),
                TransactionCode = null, // Will be set by payment gateway
                Status = "PENDING",
                PaymentType = "TRANSACTION",
                CompletedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Người dùng {UserId} đã tạo thanh toán {PaymentId} cho đơn hàng {OrderId}, Gateway: {Gateway}",
                userId,
                payment.Id,
                request.OrderId,
                request.PaymentGateway);

            // 6. Update Order payment method
            order.PaymentMethod = request.PaymentGateway.ToUpper();
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            // 7. Generate payment URL (mock - in real implementation, call payment gateway API)
            var paymentUrl = GeneratePaymentUrl(payment.Id, order.FinalPrice, request.PaymentGateway);

            // 8. Map to response
            var response = MapPaymentToResponse(payment);
            response.PaymentUrl = paymentUrl;

            return BaseResponse<PaymentResponse>.SuccessResponse(
                response,
                "Tạo thanh toán thành công. Vui lòng thanh toán qua cổng thanh toán.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo thanh toán, UserId: {UserId}, OrderId: {OrderId}", userId, request.OrderId);
            return BaseResponse<PaymentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo thanh toán");
        }
    }

    /// <summary>
    /// UC28: Webhook xử lý thanh toán
    /// </summary>
    public async Task<BaseResponse<PaymentResponse>> ProcessPaymentWebhookAsync(
        PaymentWebhookRequest request)
    {
        try
        {
            // 1. Find payment by transaction code or payment ID
            Payment? payment = null;

            // Thử tìm bằng TransactionCode trước (nếu có)
            if (!string.IsNullOrEmpty(request.TransactionCode))
            {
                payment = await _paymentRepository.GetByTransactionCodeAsync(request.TransactionCode);
            }

            // Nếu không tìm thấy và có PaymentId, thử tìm bằng PaymentId
            if (payment == null && request.PaymentId.HasValue)
            {
                payment = await _paymentRepository.GetByIdForUpdateAsync(request.PaymentId.Value);
            }

            if (payment == null)
            {
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Không tìm thấy thanh toán với thông tin đã cung cấp");
            }

            // 2. Validate payment status
            if (payment.Status != "PENDING")
            {
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Thanh toán đã được xử lý");
            }

            // 3. Validate amount
            if (payment.Amount != request.Amount)
            {
                _logger.LogWarning(
                    "Số tiền thanh toán không khớp. PaymentId: {PaymentId}, Expected: {Expected}, Received: {Received}",
                    payment.Id,
                    payment.Amount,
                    request.Amount);
                return BaseResponse<PaymentResponse>.FailureResponse(
                    "Số tiền thanh toán không khớp");
            }

            // 4. Update payment status
            payment.TransactionCode = request.TransactionCode;
            payment.Status = request.Status.ToUpper() == "SUCCESS" ? "SUCCESS" : "FAILED";
            payment.CompletedAt = request.Status.ToUpper() == "SUCCESS" ? DateTime.UtcNow : null;

            await _paymentRepository.UpdateAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // 5. If payment successful, handle based on payment type
            if (payment.Status == "SUCCESS")
            {
                if (payment.PaymentType == "PACKAGE" && payment.PackageId.HasValue)
                {
                    // UC26: Handle package payment - create or update UserPackageCredits
                    await HandlePackagePaymentSuccessAsync(payment);
                }
                else if (payment.PaymentType == "TRANSACTION" && payment.OrderId.HasValue)
                {
                    // UC28: Handle order payment - update order status
                    await HandleOrderPaymentSuccessAsync(payment);
                }
            }

            // 7. Map to response
            var response = MapPaymentToResponse(payment);

            return BaseResponse<PaymentResponse>.SuccessResponse(
                response,
                "Xử lý thanh toán thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý webhook thanh toán, TransactionCode: {TransactionCode}", request.TransactionCode);
            return BaseResponse<PaymentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi xử lý thanh toán");
        }
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.Id,
            LeadId = order.LeadId,
            PostId = order.PostId,
            PostTitle = order.Post?.Title ?? string.Empty,
            BuyerId = order.BuyerId,
            BuyerName = order.Buyer?.FullName ?? order.Buyer?.Email ?? string.Empty,
            SellerId = order.SellerId,
            SellerName = order.Seller?.FullName ?? order.Seller?.Email ?? string.Empty,
            StaffId = order.StaffId,
            StaffName = order.Staff?.FullName ?? order.Staff?.Email,
            FinalPrice = order.FinalPrice,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            PaidAt = order.PaidAt,
            CompletedAt = order.CompletedAt,
            CreatedAt = order.CreatedAt
        };
    }

    private PaymentResponse MapPaymentToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            PaymentGateway = payment.PaymentGateway,
            TransactionCode = payment.TransactionCode,
            Status = payment.Status,
            PaymentType = payment.PaymentType,
            CompletedAt = payment.CompletedAt
        };
    }

    private string GeneratePaymentUrl(Guid paymentId, decimal amount, string paymentGateway)
    {
        // Mock implementation - in real scenario, call payment gateway API
        // This would return the actual payment URL from VNPay, MoMo, etc.
        return $"/payment/{paymentId}/redirect?gateway={paymentGateway}&amount={amount}";
    }

    /// <summary>
    /// UC26: Xử lý thanh toán gói tin thành công
    /// </summary>
    private async Task HandlePackagePaymentSuccessAsync(Payment payment)
    {
        try
        {
            if (!payment.PackageId.HasValue)
            {
                _logger.LogWarning("Payment {PaymentId} không có PackageId", payment.Id);
                return;
            }

            // 1. Get package
            var package = await _packageRepository.GetByIdAsync(payment.PackageId.Value);
            if (package == null)
            {
                _logger.LogError("Không tìm thấy gói tin {PackageId} cho payment {PaymentId}", payment.PackageId, payment.Id);
                return;
            }

            // 2. Get or create UserPackageCredits
            var userCredits = await _userPackageCreditsRepository.GetByUserAndPackageAsync(
                payment.UserId,
                payment.PackageId.Value);

            if (userCredits == null)
            {
                // Create new UserPackageCredits
                userCredits = new UserPackageCredits
                {
                    Id = Guid.NewGuid(),
                    UserId = payment.UserId,
                    PackageId = payment.PackageId.Value,
                    CreditsRemaining = package.CreditsCount,
                    TotalCredits = package.CreditsCount,
                    PurchasedAt = DateTime.UtcNow,
                    ExpiresAt = null // Không có thời hạn, hoặc có thể set theo yêu cầu
                };

                await _userPackageCreditsRepository.CreateAsync(userCredits);
                _logger.LogInformation(
                    "Tạo UserPackageCredits mới cho User {UserId}, Package {PackageId}, Credits: {Credits}",
                    payment.UserId,
                    payment.PackageId.Value,
                    package.CreditsCount);
            }
            else
            {
                // Update existing UserPackageCredits - cộng thêm credits
                userCredits.CreditsRemaining += package.CreditsCount;
                userCredits.TotalCredits += package.CreditsCount;
                userCredits.PurchasedAt = DateTime.UtcNow; // Update last purchase date

                await _userPackageCreditsRepository.UpdateAsync(userCredits);
                _logger.LogInformation(
                    "Cập nhật UserPackageCredits cho User {UserId}, Package {PackageId}, Thêm credits: {Credits}, Tổng credits còn lại: {Remaining}",
                    payment.UserId,
                    payment.PackageId.Value,
                    package.CreditsCount,
                    userCredits.CreditsRemaining);
            }

            // 3. Update payment with UserCreditId
            payment.UserCreditId = userCredits.Id;
            await _paymentRepository.UpdateAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // 4. Save UserPackageCredits changes
            await _userPackageCreditsRepository.SaveChangesAsync();

            // 5. Create notification
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = payment.UserId,
                NotificationType = "PACKAGE_PURCHASE_SUCCESS",
                Title = "Mua gói tin thành công",
                Content = $"Bạn đã mua gói {package.Name} thành công. Bạn đã nhận {package.CreditsCount} credits.",
                RelatedId = payment.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Thanh toán gói tin thành công. PaymentId: {PaymentId}, UserId: {UserId}, PackageId: {PackageId}, Credits: {Credits}",
                payment.Id,
                payment.UserId,
                payment.PackageId.Value,
                package.CreditsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý thanh toán gói tin thành công, PaymentId: {PaymentId}", payment.Id);
            // Không throw exception để không làm gián đoạn flow
        }
    }

    /// <summary>
    /// UC28: Xử lý thanh toán đơn hàng thành công
    /// </summary>
    private async Task HandleOrderPaymentSuccessAsync(Payment payment)
    {
        try
        {
            if (!payment.OrderId.HasValue)
            {
                _logger.LogWarning("Payment {PaymentId} không có OrderId", payment.Id);
                return;
            }

            var order = await _orderRepository.GetByIdWithDetailsAsync(payment.OrderId.Value);
            if (order == null)
            {
                _logger.LogError("Không tìm thấy đơn hàng {OrderId} cho payment {PaymentId}", payment.OrderId, payment.Id);
                return;
            }

            order.Status = "PAID";
            order.PaidAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Đơn hàng {OrderId} đã được thanh toán thành công",
                order.Id);

            // Create notifications
            var buyerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = order.BuyerId,
                NotificationType = "PAYMENT_SUCCESS",
                Title = "Thanh toán thành công",
                Content = $"Thanh toán cho đơn hàng đã thành công. Đơn hàng đang được xử lý.",
                RelatedId = order.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(buyerNotification);

            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = order.SellerId,
                NotificationType = "PAYMENT_SUCCESS",
                Title = "Thanh toán thành công",
                Content = $"Người mua đã thanh toán cho đơn hàng. Đơn hàng đang được xử lý.",
                RelatedId = order.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(sellerNotification);

            await _notificationRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý thanh toán đơn hàng thành công, PaymentId: {PaymentId}", payment.Id);
            // Không throw exception để không làm gián đoạn flow
        }
    }

    /// <summary>
    /// UC05: Lấy danh sách Orders của Member (với filter và phân trang)
    /// </summary>
    public async Task<BaseResponse<PagedResponse<OrderResponse>>> GetMyOrdersAsync(
        Guid userId,
        OrderSearchRequest request)
    {
        try
        {
            // Validate request
            if (!request.IsValid())
            {
                return BaseResponse<PagedResponse<OrderResponse>>.FailureResponse(
                    "Dữ liệu không hợp lệ");
            }

            // 1. Get all orders where user is buyer or seller
            List<Order> allOrders = new List<Order>();

            // Get buyer orders
            if (request.TransactionType == null || request.TransactionType.ToUpper() == "BUY")
            {
                var buyerOrders = await _orderRepository.GetOrdersByBuyerIdAsync(userId);
                allOrders.AddRange(buyerOrders);
            }

            // Get seller orders
            if (request.TransactionType == null || request.TransactionType.ToUpper() == "SELL")
            {
                var sellerOrders = await _orderRepository.GetOrdersBySellerIdAsync(userId);
                allOrders.AddRange(sellerOrders);
            }

            // 2. Remove duplicates (if user is both buyer and seller somehow)
            allOrders = allOrders.GroupBy(o => o.Id).Select(g => g.First()).ToList();

            // 3. Get orders with details for mapping
            var ordersWithDetails = new List<Order>();
            foreach (var order in allOrders)
            {
                var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
                if (orderWithDetails != null)
                {
                    ordersWithDetails.Add(orderWithDetails);
                }
            }

            // 4. Apply filters
            var filteredOrders = ordersWithDetails.AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filteredOrders = filteredOrders.Where(o => o.Status == request.Status.ToUpper());
            }

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value.Date.AddDays(1); // Include the entire day
                filteredOrders = filteredOrders.Where(o => o.CreatedAt < toDate);
            }

            // Filter by keyword (search in post title)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Post != null && o.Post.Title.ToLower().Contains(keyword)) ||
                    (o.Post != null && !string.IsNullOrEmpty(o.Post.Description) && o.Post.Description.ToLower().Contains(keyword)));
            }

            // 5. Apply sorting
            var sortBy = request.SortBy?.ToLower() ?? "createdat";
            var sortDirection = request.SortDirection?.ToLower() ?? "desc";

            filteredOrders = sortBy switch
            {
                "createdat" => sortDirection == "asc"
                    ? filteredOrders.OrderBy(o => o.CreatedAt)
                    : filteredOrders.OrderByDescending(o => o.CreatedAt),
                "finalprice" => sortDirection == "asc"
                    ? filteredOrders.OrderBy(o => o.FinalPrice)
                    : filteredOrders.OrderByDescending(o => o.FinalPrice),
                "status" => sortDirection == "asc"
                    ? filteredOrders.OrderBy(o => o.Status)
                    : filteredOrders.OrderByDescending(o => o.Status),
                _ => filteredOrders.OrderByDescending(o => o.CreatedAt)
            };

            // 6. Get total count before pagination
            var totalCount = filteredOrders.Count();

            // 7. Apply pagination
            var pagedOrders = filteredOrders
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToList();

            // 8. Map to response
            var orderResponses = pagedOrders.Select(MapToResponse).ToList();

            // 9. Create paged response
            var pagedResponse = PagedResponse<OrderResponse>.SuccessResponse(
                orderResponses,
                request.PageNumber,
                request.PageSize,
                totalCount,
                "Lấy danh sách đơn hàng thành công");

            return BaseResponse<PagedResponse<OrderResponse>>.SuccessResponse(
                pagedResponse,
                "Lấy danh sách đơn hàng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng, UserId: {UserId}", userId);
            return BaseResponse<PagedResponse<OrderResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách đơn hàng");
        }
    }
}

