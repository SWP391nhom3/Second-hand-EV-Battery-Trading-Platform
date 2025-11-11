using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Orders;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Payment Service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPayOSService _payOSService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPayOSService payOSService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _payOSService = payOSService;
        _logger = logger;
    }

    /// <summary>
    /// UC30: Lấy lịch sử thanh toán với filter và phân trang
    /// </summary>
    public async Task<PagedResponse<PaymentDetailResponse>> GetPaymentHistoryAsync(
        Guid userId,
        PaymentSearchRequest request)
    {
        try
        {
            // Validate request
            request.IsValid();

            // Search payments
            var result = await _paymentRepository.SearchPaymentsAsync(
                userId,
                request.PaymentType,
                request.Status,
                request.PaymentGateway,
                request.FromDate,
                request.ToDate,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection);

            // Map to response
            var responses = result.Items.Select(MapToDetailResponse).ToList();

            return PagedResponse<PaymentDetailResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy lịch sử thanh toán thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử thanh toán, UserId: {UserId}", userId);
            return PagedResponse<PaymentDetailResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy lịch sử thanh toán");
        }
    }

    /// <summary>
    /// UC30: Lấy chi tiết thanh toán
    /// </summary>
    public async Task<BaseResponse<PaymentDetailResponse>> GetPaymentByIdAsync(
        Guid userId,
        Guid paymentId)
    {
        try
        {
            // Get payment with details
            var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
            if (payment == null)
            {
                return BaseResponse<PaymentDetailResponse>.FailureResponse(
                    "Không tìm thấy thanh toán");
            }

            // Validate ownership
            if (payment.UserId != userId)
            {
                return BaseResponse<PaymentDetailResponse>.FailureResponse(
                    "Bạn không có quyền xem thanh toán này");
            }

            // Map to response
            var response = MapToDetailResponse(payment);

            return BaseResponse<PaymentDetailResponse>.SuccessResponse(
                response,
                "Lấy chi tiết thanh toán thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết thanh toán, UserId: {UserId}, PaymentId: {PaymentId}", userId, paymentId);
            return BaseResponse<PaymentDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết thanh toán");
        }
    }

    /// <summary>
    /// Map Payment entity to PaymentDetailResponse
    /// </summary>
    private PaymentDetailResponse MapToDetailResponse(Domain.Entities.Payment payment)
    {
        var response = new PaymentDetailResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            PackageId = payment.PackageId,
            PackageName = payment.Package?.Name,
            CreditsCount = payment.Package?.CreditsCount,
            UserId = payment.UserId,
            Amount = payment.Amount,
            PaymentGateway = payment.PaymentGateway,
            TransactionCode = payment.TransactionCode,
            Status = payment.Status,
            PaymentType = payment.PaymentType,
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };

        // Add Order info if payment is for transaction
        if (payment.PaymentType == "TRANSACTION" && payment.Order != null)
        {
            response.OrderInfo = new OrderInfo
            {
                OrderId = payment.Order.Id,
                PostTitle = payment.Order.Post?.Title ?? string.Empty,
                SellerName = payment.Order.Seller?.FullName ?? payment.Order.Seller?.Email ?? string.Empty,
                FinalPrice = payment.Order.FinalPrice
            };
        }

        return response;
    }

    /// <summary>
    /// Lấy PayOS payment link và QR code
    /// </summary>
    public async Task<BaseResponse<PayOSPaymentLinkResponse>> GetPayOSPaymentLinkAsync(
        Guid userId,
        Guid paymentId)
    {
        try
        {
            // Get payment with details
            var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
            if (payment == null)
            {
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    "Không tìm thấy thanh toán");
            }

            // Validate ownership
            if (payment.UserId != userId)
            {
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    "Bạn không có quyền xem thanh toán này");
            }

            // Validate payment gateway
            if (payment.PaymentGateway != "PAYOS")
            {
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    "Payment gateway không phải PayOS");
            }

            // Validate payment status
            if (payment.Status != "PENDING")
            {
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    "Chỉ có thể tạo payment link cho thanh toán đang chờ thanh toán");
            }

            // Convert GUID to long for PayOS orderCode
            // PayOS requires orderCode to be a number, so we use a hash of the GUID
            var orderCode = ConvertGuidToOrderCode(paymentId);

            // Build description
            var description = BuildPaymentDescription(payment);

            // Build return URLs - PayOSService will use default URLs from configuration
            // These URLs will redirect to frontend with paymentId
            var returnUrl = $"http://localhost:3000/payment/{paymentId}/success";
            var cancelUrl = $"http://localhost:3000/payment/{paymentId}/cancel";

            // Call PayOS service to create payment link
            var payOSResponse = await _payOSService.CreatePaymentLinkAsync(
                orderCode,
                payment.Amount,
                description,
                returnUrl,
                cancelUrl);

            if (!payOSResponse.Success)
            {
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    payOSResponse.Message ?? "Không thể tạo payment link từ PayOS");
            }

            _logger.LogInformation(
                "PayOS payment link created successfully. PaymentId: {PaymentId}, OrderCode: {OrderCode}",
                paymentId,
                orderCode);

            return BaseResponse<PayOSPaymentLinkResponse>.SuccessResponse(
                payOSResponse.Data!,
                "Tạo payment link thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy PayOS payment link. UserId: {UserId}, PaymentId: {PaymentId}", userId, paymentId);
            return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy PayOS payment link");
        }
    }

    /// <summary>
    /// Convert GUID to long orderCode for PayOS
    /// PayOS requires orderCode to be a positive number
    /// </summary>
    private long ConvertGuidToOrderCode(Guid guid)
    {
        // Use first 8 bytes of GUID to create a positive long
        var bytes = guid.ToByteArray();
        var longValue = BitConverter.ToInt64(bytes, 0);
        
        // Ensure positive number (PayOS requires positive orderCode)
        // Use absolute value and add a large number to ensure it's positive and unique
        return Math.Abs(longValue) + 1000000000; // Add 1 billion to ensure positive
    }

    /// <summary>
    /// Build payment description based on payment type
    /// </summary>
    private string BuildPaymentDescription(Domain.Entities.Payment payment)
    {
        if (payment.PaymentType == "PACKAGE" && payment.Package != null)
        {
            return $"Mua gói tin: {payment.Package.Name} - {payment.Package.CreditsCount} credits";
        }
        else if (payment.PaymentType == "TRANSACTION" && payment.Order != null)
        {
            var postTitle = payment.Order.Post?.Title ?? "Sản phẩm";
            return $"Thanh toán đơn hàng: {postTitle}";
        }
        else
        {
            return $"Thanh toán số tiền {payment.Amount:N0} VND";
        }
    }
}


