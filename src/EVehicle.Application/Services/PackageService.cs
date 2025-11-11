using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Packages;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Package Service implementation
/// </summary>
public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUserPackageCreditsRepository _userPackageCreditsRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PackageService> _logger;

    public PackageService(
        IPackageRepository packageRepository,
        IUserPackageCreditsRepository userPackageCreditsRepository,
        IPaymentRepository paymentRepository,
        ILogger<PackageService> logger)
    {
        _packageRepository = packageRepository;
        _userPackageCreditsRepository = userPackageCreditsRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<List<PackageResponse>>> GetPackagesWithCreditsAsync(Guid? userId)
    {
        try
        {
            var packages = await _packageRepository.GetAllActiveAsync();

            var responses = new List<PackageResponse>();

            foreach (var package in packages)
            {
                int? creditsRemaining = null;

                // Nếu có userId, lấy số credits còn lại
                if (userId.HasValue)
                {
                    var userCredits = await _userPackageCreditsRepository.GetByUserAndPackageAsync(
                        userId.Value,
                        package.PackageId);

                    if (userCredits != null)
                    {
                        creditsRemaining = userCredits.CreditsRemaining;
                    }
                }

                responses.Add(new PackageResponse
                {
                    Id = package.PackageId,
                    Text = $"{package.Name} ({package.CreditsCount} credits) - {package.Price:N0} VNĐ",
                    Name = package.Name,
                    Price = package.Price,
                    CreditsCount = package.CreditsCount,
                    CreditsRemaining = creditsRemaining,
                    PriorityLevel = package.PriorityLevel,
                    MaxImages = package.MaxImages
                });
            }

            return BaseResponse<List<PackageResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin, UserId: {UserId}", userId);
            return BaseResponse<List<PackageResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách gói tin");
        }
    }

    public async Task<BaseResponse<List<UserPackageCreditsResponse>>> GetUserPackagesWithCreditsAsync(Guid userId)
    {
        try
        {
            // 1. Lấy danh sách user package credits
            var userCredits = await _userPackageCreditsRepository.GetByUserIdAsync(userId);

            // 2. Map sang response DTO và sắp xếp
            var responses = userCredits
                .Select(uc => new UserPackageCreditsResponse
                {
                    PackageId = uc.PackageId,
                    PackageName = uc.Package.Name,
                    Price = uc.Package.Price,
                    CreditsCount = uc.Package.CreditsCount,
                    CreditsRemaining = uc.CreditsRemaining,
                    TotalCredits = uc.TotalCredits,
                    PriorityLevel = uc.Package.PriorityLevel,
                    MaxImages = uc.Package.MaxImages,
                    PurchasedAt = uc.PurchasedAt,
                    ExpiresAt = uc.ExpiresAt
                })
                .OrderByDescending(r => r.HasCredits) // Gói có credits đứng trước
                .ThenByDescending(r => r.PriorityLevel) // Sắp xếp theo priority
                .ThenByDescending(r => r.PurchasedAt) // Sau đó theo thời gian mua
                .ToList();

            return BaseResponse<List<UserPackageCreditsResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách gói tin của user thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin của user, UserId: {UserId}", userId);
            return BaseResponse<List<UserPackageCreditsResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách gói tin của user");
        }
    }

    /// <summary>
    /// UC26: Mua gói tin
    /// </summary>
    public async Task<BaseResponse<PackagePurchaseResponse>> PurchasePackageAsync(
        Guid userId,
        PackagePurchaseRequest request)
    {
        try
        {
            // 1. Validate package exists
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                return BaseResponse<PackagePurchaseResponse>.FailureResponse(
                    "Gói tin không tồn tại");
            }

            // 2. Validate package is active
            if (!package.IsActive)
            {
                return BaseResponse<PackagePurchaseResponse>.FailureResponse(
                    "Gói tin không còn hoạt động");
            }

            // 3. Check if there's a pending payment for this package
            var existingPayment = await _paymentRepository.GetByPackageIdAndUserIdAsync(
                request.PackageId,
                userId);
            if (existingPayment != null && existingPayment.Status == "PENDING")
            {
                // Return existing payment URL if available
                var existingResponse = new PackagePurchaseResponse
                {
                    PaymentId = existingPayment.Id,
                    PackageId = package.PackageId,
                    PackageName = package.Name,
                    Amount = existingPayment.Amount,
                    PaymentGateway = existingPayment.PaymentGateway,
                    Status = existingPayment.Status,
                    PaymentUrl = GeneratePaymentUrl(existingPayment.Id, existingPayment.Amount, existingPayment.PaymentGateway),
                    CreditsCount = package.CreditsCount
                };

                return BaseResponse<PackagePurchaseResponse>.SuccessResponse(
                    existingResponse,
                    "Đã có thanh toán đang chờ xử lý");
            }

            // 4. Create Payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = request.PackageId,
                Amount = package.Price,
                PaymentGateway = request.PaymentGateway.ToUpper(),
                TransactionCode = null, // Will be set by payment gateway
                Status = "PENDING",
                PaymentType = "PACKAGE",
                CompletedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Người dùng {UserId} đã tạo thanh toán {PaymentId} cho gói tin {PackageId}, Gateway: {Gateway}",
                userId,
                payment.Id,
                request.PackageId,
                request.PaymentGateway);

            // 5. Generate payment URL (mock - in real implementation, call payment gateway API)
            var paymentUrl = GeneratePaymentUrl(payment.Id, package.Price, request.PaymentGateway);

            // 6. Map to response
            var response = new PackagePurchaseResponse
            {
                PaymentId = payment.Id,
                PackageId = package.PackageId,
                PackageName = package.Name,
                Amount = package.Price,
                PaymentGateway = request.PaymentGateway.ToUpper(),
                Status = "PENDING",
                PaymentUrl = paymentUrl,
                CreditsCount = package.CreditsCount
            };

            return BaseResponse<PackagePurchaseResponse>.SuccessResponse(
                response,
                "Tạo thanh toán thành công. Vui lòng thanh toán qua cổng thanh toán.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi mua gói tin, UserId: {UserId}, PackageId: {PackageId}", userId, request.PackageId);
            return BaseResponse<PackagePurchaseResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi mua gói tin");
        }
    }

    /// <summary>
    /// UC48: Lấy danh sách gói tin với phân trang (Admin)
    /// </summary>
    public async Task<PagedResponse<PackageDetailResponse>> GetPackagesAsync(PackageSearchRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _packageRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection,
                request.IsActive);

            var responses = result.Items.Select(MapToDetailResponse).ToList();

            return PagedResponse<PackageDetailResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin");
            return PagedResponse<PackageDetailResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách gói tin");
        }
    }

    /// <summary>
    /// UC48: Lấy chi tiết gói tin theo ID (Admin)
    /// </summary>
    public async Task<BaseResponse<PackageDetailResponse>> GetPackageByIdAsync(int packageId)
    {
        try
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Không tìm thấy gói tin");
            }

            var response = MapToDetailResponse(package);
            return BaseResponse<PackageDetailResponse>.SuccessResponse(
                response,
                "Lấy chi tiết gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết gói tin, PackageId: {PackageId}", packageId);
            return BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết gói tin");
        }
    }

    /// <summary>
    /// UC48: Tạo gói tin mới (Admin)
    /// </summary>
    public async Task<BaseResponse<PackageDetailResponse>> CreatePackageAsync(PackageCreateRequest request)
    {
        try
        {
            // 1. Validate tên gói tin không trùng
            var existsByName = await _packageRepository.ExistsByNameAsync(request.Name);
            if (existsByName)
            {
                return BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Tên gói tin đã tồn tại");
            }

            // 2. Tạo entity
            var package = new PackageDefinition
            {
                Name = request.Name,
                Price = request.Price,
                CreditsCount = request.CreditsCount,
                PriorityLevel = request.PriorityLevel,
                MaxImages = request.MaxImages,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Lưu vào database
            await _packageRepository.CreateAsync(package);
            await _packageRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Admin đã tạo gói tin mới: {PackageId} - {PackageName}",
                package.PackageId,
                package.Name);

            // 4. Map to response
            var response = MapToDetailResponse(package);
            return BaseResponse<PackageDetailResponse>.SuccessResponse(
                response,
                "Tạo gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo gói tin");
            return BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo gói tin");
        }
    }

    /// <summary>
    /// UC48: Cập nhật gói tin (Admin)
    /// </summary>
    public async Task<BaseResponse<PackageDetailResponse>> UpdatePackageAsync(
        int packageId,
        PackageUpdateRequest request)
    {
        try
        {
            // 1. Validate package exists (có tracking để update)
            var package = await _packageRepository.GetByIdForUpdateAsync(packageId);
            if (package == null)
            {
                return BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Không tìm thấy gói tin");
            }

            // 2. Validate tên gói tin không trùng (trừ chính nó)
            var existsByName = await _packageRepository.ExistsByNameAsync(request.Name, packageId);
            if (existsByName)
            {
                return BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Tên gói tin đã tồn tại");
            }

            // 3. Update entity
            package.Name = request.Name;
            package.Price = request.Price;
            package.CreditsCount = request.CreditsCount;
            package.PriorityLevel = request.PriorityLevel;
            package.MaxImages = request.MaxImages;
            package.IsActive = request.IsActive;

            // 4. Lưu vào database
            await _packageRepository.UpdateAsync(package);
            await _packageRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Admin đã cập nhật gói tin: {PackageId} - {PackageName}",
                package.PackageId,
                package.Name);

            // 5. Map to response
            var response = MapToDetailResponse(package);
            return BaseResponse<PackageDetailResponse>.SuccessResponse(
                response,
                "Cập nhật gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật gói tin, PackageId: {PackageId}", packageId);
            return BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật gói tin");
        }
    }

    /// <summary>
    /// UC48: Kích hoạt/vô hiệu hóa gói tin (Admin)
    /// </summary>
    public async Task<BaseResponse> TogglePackageStatusAsync(int packageId)
    {
        try
        {
            // 1. Validate package exists (có tracking để update)
            var package = await _packageRepository.GetByIdForUpdateAsync(packageId);
            if (package == null)
            {
                return BaseResponse.FailureResponse("Không tìm thấy gói tin");
            }

            // 2. Toggle status
            package.IsActive = !package.IsActive;

            // 3. Lưu vào database
            await _packageRepository.UpdateAsync(package);
            await _packageRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Admin đã {Action} gói tin: {PackageId} - {PackageName}",
                package.IsActive ? "kích hoạt" : "vô hiệu hóa",
                package.PackageId,
                package.Name);

            return BaseResponse.SuccessResponse(
                $"Đã {(package.IsActive ? "kích hoạt" : "vô hiệu hóa")} gói tin thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái gói tin, PackageId: {PackageId}", packageId);
            return BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi thay đổi trạng thái gói tin");
        }
    }

    /// <summary>
    /// Map PackageDefinition entity sang PackageDetailResponse
    /// </summary>
    private PackageDetailResponse MapToDetailResponse(PackageDefinition package)
    {
        return new PackageDetailResponse
        {
            Id = package.PackageId,
            Name = package.Name,
            Price = package.Price,
            CreditsCount = package.CreditsCount,
            PriorityLevel = package.PriorityLevel,
            MaxImages = package.MaxImages,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt
        };
    }

    /// <summary>
    /// Generate payment URL (mock implementation)
    /// In real scenario, this would call payment gateway API (VNPay, MoMo, etc.)
    /// </summary>
    private string GeneratePaymentUrl(Guid paymentId, decimal amount, string paymentGateway)
    {
        // Mock implementation - in real scenario, call payment gateway API
        // This would return the actual payment URL from VNPay, MoMo, etc.
        return $"/payment/{paymentId}/redirect?gateway={paymentGateway}&amount={amount}";
    }
}

