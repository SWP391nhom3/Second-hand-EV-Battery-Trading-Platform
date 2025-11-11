using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Contracts;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Contract Service implementation
/// </summary>
public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractTemplateRepository _contractTemplateRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<ContractService> _logger;

    public ContractService(
        IContractRepository contractRepository,
        IContractTemplateRepository contractTemplateRepository,
        IOrderRepository orderRepository,
        ILeadRepository leadRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ILogger<ContractService> logger)
    {
        _contractRepository = contractRepository;
        _contractTemplateRepository = contractTemplateRepository;
        _orderRepository = orderRepository;
        _leadRepository = leadRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// UC43: Lấy danh sách mẫu hợp đồng
    /// </summary>
    public async Task<BaseResponse<List<ContractTemplateResponse>>> GetContractTemplatesAsync(int? categoryId = null)
    {
        try
        {
            var templates = await _contractTemplateRepository.GetActiveTemplatesAsync(categoryId);

            var responses = templates.Select(t => new ContractTemplateResponse
            {
                TemplateId = t.TemplateId,
                TemplateName = t.TemplateName,
                TemplateContent = t.TemplateContent,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            }).ToList();

            return BaseResponse<List<ContractTemplateResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách mẫu hợp đồng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách mẫu hợp đồng");
            return BaseResponse<List<ContractTemplateResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách mẫu hợp đồng");
        }
    }

    /// <summary>
    /// UC43: Staff tạo hợp đồng từ mẫu
    /// </summary>
    public async Task<BaseResponse<ContractResponse>> CreateContractAsync(
        Guid staffId,
        ContractCreateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Validate Template exists
            var template = await _contractTemplateRepository.GetByIdAsync(request.ContractTemplateId);
            if (template == null || !template.IsActive)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Mẫu hợp đồng không tồn tại hoặc không hoạt động");
            }

            // 3. Validate Order or Lead exists
            Order? order = null;
            Lead? lead = null;

            if (request.OrderId.HasValue)
            {
                order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId.Value);
                if (order == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy đơn hàng");
                }

                // Validate Order belongs to this Staff
                if (order.StaffId != staffId)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Bạn không có quyền tạo hợp đồng cho đơn hàng này");
                }

                // Validate Order is PAID
                if (order.Status != "PAID")
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Đơn hàng phải ở trạng thái PAID để tạo hợp đồng");
                }
            }
            else if (request.LeadId.HasValue)
            {
                lead = await _leadRepository.GetByIdWithDetailsAsync(request.LeadId.Value);
                if (lead == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy Lead");
                }

                // Validate Lead belongs to this Staff
                if (lead.StaffId != staffId)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Bạn không có quyền tạo hợp đồng cho Lead này");
                }

                // Validate Lead is SUCCESSFUL
                if (lead.Status != "SUCCESSFUL")
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Lead phải ở trạng thái SUCCESSFUL để tạo hợp đồng");
                }
            }
            else
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Phải có OrderId hoặc LeadId");
            }

            // 4. Check if Contract already exists
            if (request.OrderId.HasValue)
            {
                var existingContract = await _contractRepository.GetByOrderIdAsync(request.OrderId.Value);
                if (existingContract != null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Hợp đồng đã tồn tại cho đơn hàng này");
                }
            }
            else if (request.LeadId.HasValue)
            {
                var existingContract = await _contractRepository.GetByLeadIdAsync(request.LeadId.Value);
                if (existingContract != null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Hợp đồng đã tồn tại cho Lead này");
                }
            }

            // 5. Auto-fill contract content from template
            if (string.IsNullOrEmpty(template.TemplateContent) && string.IsNullOrEmpty(request.ContractContent))
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Mẫu hợp đồng không có nội dung");
            }

            string contractContent = request.ContractContent ?? template.TemplateContent ?? string.Empty;

            // Replace placeholders with actual data
            if (order != null)
            {
                // Validate required data exists
                if (order.Buyer == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin người mua trong đơn hàng");
                }
                if (order.Seller == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin người bán trong đơn hàng");
                }
                if (order.Post == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin bài đăng trong đơn hàng");
                }

                // Validate price
                decimal price = order.FinalPrice;
                if (price <= 0)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Giá đơn hàng không hợp lệ");
                }

                contractContent = ReplaceContractPlaceholders(
                    contractContent,
                    order.Buyer,
                    order.Seller,
                    order.Post,
                    price);
            }
            else if (lead != null)
            {
                // Validate required data exists
                if (lead.Buyer == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin người mua trong Lead");
                }
                if (lead.Post == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin bài đăng trong Lead");
                }
                if (lead.Post.User == null)
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy thông tin người bán trong Lead");
                }

                var seller = lead.Post.User;
                
                // Validate price
                decimal price = 0;
                if (lead.FinalPrice.HasValue)
                {
                    price = lead.FinalPrice.Value;
                }
                else if (lead.Post != null && lead.Post.Price > 0)
                {
                    price = lead.Post.Price;
                }
                else
                {
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không tìm thấy giá sản phẩm trong Lead hoặc bài đăng");
                }

                contractContent = ReplaceContractPlaceholders(
                    contractContent,
                    lead.Buyer,
                    seller,
                    lead.Post,
                    price);
            }

            // 6. Validate contract content is not empty
            if (string.IsNullOrWhiteSpace(contractContent))
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Nội dung hợp đồng không được để trống");
            }

            // 7. Create Contract
            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                LeadId = request.LeadId,
                ContractTemplateId = request.ContractTemplateId,
                CreatedBy = staffId,
                ContractContent = contractContent,
                BuyerSignature = null,
                SellerSignature = null,
                BuyerSignedAt = null,
                SellerSignedAt = null,
                ContractPdfUrl = null,
                Status = "DRAFT",
                SignedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            await _contractRepository.CreateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã tạo hợp đồng {ContractId} từ mẫu {TemplateId}",
                staffId,
                contract.Id,
                request.ContractTemplateId);

            // 8. Generate PDF and update contract
            try
            {
                var pdfUrl = await GenerateContractPdfAsync(contract.Id, contractContent);
                contract.ContractPdfUrl = pdfUrl;
                contract.Status = "PENDING_SIGNATURE";
                await _contractRepository.UpdateAsync(contract);
                await _contractRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể tạo PDF cho hợp đồng {ContractId}", contract.Id);
                // Continue without PDF, can generate later
            }

            // 9. Create notifications
            if (order != null)
            {
                var buyerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = order.BuyerId,
                    NotificationType = "CONTRACT_CREATED",
                    Title = "Hợp đồng đã được tạo",
                    Content = $"Hợp đồng mua bán đã được tạo. Vui lòng xem xét và ký.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(buyerNotification);

                var sellerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = order.SellerId,
                    NotificationType = "CONTRACT_CREATED",
                    Title = "Hợp đồng đã được tạo",
                    Content = $"Hợp đồng mua bán đã được tạo. Vui lòng xem xét và ký.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(sellerNotification);
            }
            else if (lead != null)
            {
                var buyerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = lead.BuyerId,
                    NotificationType = "CONTRACT_CREATED",
                    Title = "Hợp đồng đã được tạo",
                    Content = $"Hợp đồng mua bán đã được tạo. Vui lòng xem xét và ký.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(buyerNotification);

                if (lead.Post != null)
                {
                    var sellerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = lead.Post.UserId,
                        NotificationType = "CONTRACT_CREATED",
                        Title = "Hợp đồng đã được tạo",
                        Content = $"Hợp đồng mua bán đã được tạo. Vui lòng xem xét và ký.",
                        RelatedId = contract.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(sellerNotification);
                }
            }

            await _notificationRepository.SaveChangesAsync();

            // 10. Get contract with details for response
            var contractWithDetails = await _contractRepository.GetByIdWithDetailsAsync(contract.Id);
            if (contractWithDetails == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng vừa tạo");
            }

            // 11. Map to response
            var response = MapToResponse(contractWithDetails);

            return BaseResponse<ContractResponse>.SuccessResponse(
                response,
                "Tạo hợp đồng thành công. Đã gửi cho cả hai bên để ký.");
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Lỗi null reference khi tạo hợp đồng, StaffId: {StaffId}, ParamName: {ParamName}", 
                staffId, ex.ParamName);
            return BaseResponse<ContractResponse>.FailureResponse(
                $"Thiếu thông tin bắt buộc: {ex.ParamName}. Vui lòng kiểm tra lại dữ liệu Lead/Order.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo hợp đồng, StaffId: {StaffId}, Exception: {Exception}", 
                staffId, ex.ToString());
            return BaseResponse<ContractResponse>.FailureResponse(
                ex,
                $"Đã xảy ra lỗi khi tạo hợp đồng: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy chi tiết hợp đồng
    /// </summary>
    public async Task<BaseResponse<ContractResponse>> GetContractByIdAsync(Guid contractId, Guid? userId = null)
    {
        try
        {
            var contract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (contract == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng");
            }

            // Validate user has permission to view this contract
            if (userId.HasValue)
            {
                bool hasPermission = false;

                if (contract.OrderId.HasValue && contract.Order != null)
                {
                    hasPermission = contract.Order.BuyerId == userId.Value ||
                                   contract.Order.SellerId == userId.Value ||
                                   contract.Order.StaffId == userId.Value;
                }
                else if (contract.LeadId.HasValue && contract.Lead != null)
                {
                    hasPermission = contract.Lead.BuyerId == userId.Value ||
                                   contract.Lead.Post?.UserId == userId.Value ||
                                   contract.Lead.StaffId == userId.Value;
                }

                if (!hasPermission)
                {
                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    if (user == null || user.Role != "ADMIN")
                    {
                        return BaseResponse<ContractResponse>.FailureResponse(
                            "Bạn không có quyền xem hợp đồng này");
                    }
                }
            }

            var response = MapToResponse(contract);

            return BaseResponse<ContractResponse>.SuccessResponse(
                response,
                "Lấy chi tiết hợp đồng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết hợp đồng, ContractId: {ContractId}", contractId);
            return BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết hợp đồng");
        }
    }

    /// <summary>
    /// UC29: Người mua/người bán ký hợp đồng
    /// </summary>
    public async Task<BaseResponse<ContractResponse>> SignContractAsync(
        Guid userId,
        Guid contractId,
        ContractSignRequest request)
    {
        try
        {
            // 1. Get contract with details
            var contract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (contract == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng");
            }

            // 2. Validate contract status
            if (contract.Status != "PENDING_SIGNATURE" && contract.Status != "DRAFT")
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Hợp đồng không ở trạng thái chờ ký");
            }

            // 3. Determine if user is buyer or seller
            bool isBuyer = false;
            bool isSeller = false;

            if (contract.OrderId.HasValue && contract.Order != null)
            {
                isBuyer = contract.Order.BuyerId == userId;
                isSeller = contract.Order.SellerId == userId;
            }
            else if (contract.LeadId.HasValue && contract.Lead != null)
            {
                isBuyer = contract.Lead.BuyerId == userId;
                isSeller = contract.Lead.Post?.UserId == userId;
            }

            if (!isBuyer && !isSeller)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Bạn không có quyền ký hợp đồng này");
            }

            // 4. Check if already signed
            if (isBuyer && contract.BuyerSignature != null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Bạn đã ký hợp đồng này");
            }

            if (isSeller && contract.SellerSignature != null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Bạn đã ký hợp đồng này");
            }

            // 5. Save signature
            if (isBuyer)
            {
                contract.BuyerSignature = request.Signature;
                contract.BuyerSignedAt = DateTime.UtcNow;
            }
            else if (isSeller)
            {
                contract.SellerSignature = request.Signature;
                contract.SellerSignedAt = DateTime.UtcNow;
            }

            // 6. Check if both parties have signed
            bool bothSigned = contract.BuyerSignature != null && contract.SellerSignature != null;

            if (bothSigned)
            {
                contract.Status = "SIGNED";
                contract.SignedAt = DateTime.UtcNow;

                // Regenerate PDF with signatures
                try
                {
                    var pdfUrl = await GenerateContractPdfAsync(contractId, contract.ContractContent ?? string.Empty);
                    contract.ContractPdfUrl = pdfUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể tạo PDF cho hợp đồng đã ký {ContractId}", contractId);
                }
            }

            // 7. Save changes
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Người dùng {UserId} đã ký hợp đồng {ContractId}. Both signed: {BothSigned}",
                userId,
                contractId,
                bothSigned);

            // 8. Create notifications if both signed
            if (bothSigned)
            {
                if (contract.OrderId.HasValue && contract.Order != null)
                {
                    var buyerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = contract.Order.BuyerId,
                        NotificationType = "CONTRACT_SIGNED",
                        Title = "Hợp đồng đã được ký",
                        Content = $"Hợp đồng đã được cả hai bên ký. Bạn có thể tải xuống file PDF.",
                        RelatedId = contract.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(buyerNotification);

                    var sellerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = contract.Order.SellerId,
                        NotificationType = "CONTRACT_SIGNED",
                        Title = "Hợp đồng đã được ký",
                        Content = $"Hợp đồng đã được cả hai bên ký. Bạn có thể tải xuống file PDF.",
                        RelatedId = contract.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(sellerNotification);
                }

                await _notificationRepository.SaveChangesAsync();
            }

            // 9. Get updated contract with details
            var updatedContract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (updatedContract == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng sau khi cập nhật");
            }

            // 10. Map to response
            var response = MapToResponse(updatedContract);

            string message = bothSigned
                ? "Ký hợp đồng thành công. Hợp đồng đã được cả hai bên ký."
                : "Ký hợp đồng thành công. Đang chờ bên còn lại ký.";

            return BaseResponse<ContractResponse>.SuccessResponse(
                response,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ký hợp đồng, UserId: {UserId}, ContractId: {ContractId}", userId, contractId);
            return BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi ký hợp đồng");
        }
    }

    /// <summary>
    /// Tải xuống file PDF hợp đồng
    /// </summary>
    public async Task<BaseResponse<string>> GetContractPdfUrlAsync(Guid contractId, Guid? userId = null)
    {
        try
        {
            var contract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (contract == null)
            {
                return BaseResponse<string>.FailureResponse(
                    "Không tìm thấy hợp đồng");
            }

            // Validate user has permission
            if (userId.HasValue)
            {
                bool hasPermission = false;

                if (contract.OrderId.HasValue && contract.Order != null)
                {
                    hasPermission = contract.Order.BuyerId == userId.Value ||
                                   contract.Order.SellerId == userId.Value;
                }
                else if (contract.LeadId.HasValue && contract.Lead != null)
                {
                    hasPermission = contract.Lead.BuyerId == userId.Value ||
                                   contract.Lead.Post?.UserId == userId.Value;
                }

                if (!hasPermission)
                {
                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    if (user == null || (user.Role != "ADMIN" && user.Role != "STAFF"))
                    {
                        return BaseResponse<string>.FailureResponse(
                            "Bạn không có quyền tải xuống hợp đồng này");
                    }
                }
            }

            if (string.IsNullOrEmpty(contract.ContractPdfUrl))
            {
                return BaseResponse<string>.FailureResponse(
                    "File PDF hợp đồng chưa được tạo");
            }

            return BaseResponse<string>.SuccessResponse(
                contract.ContractPdfUrl,
                "Lấy URL PDF hợp đồng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy URL PDF hợp đồng, ContractId: {ContractId}", contractId);
            return BaseResponse<string>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy URL PDF hợp đồng");
        }
    }

    private ContractResponse MapToResponse(Contract contract)
    {
        return new ContractResponse
        {
            ContractId = contract.Id,
            OrderId = contract.OrderId,
            LeadId = contract.LeadId,
            ContractTemplateId = contract.ContractTemplateId,
            TemplateName = contract.ContractTemplate?.TemplateName,
            CreatedBy = contract.CreatedBy,
            CreatedByName = contract.CreatedByUser?.FullName ?? contract.CreatedByUser?.Email,
            ContractContent = contract.ContractContent,
            Status = contract.Status,
            IsBuyerSigned = contract.BuyerSignature != null,
            IsSellerSigned = contract.SellerSignature != null,
            BuyerSignedAt = contract.BuyerSignedAt,
            SellerSignedAt = contract.SellerSignedAt,
            SignedAt = contract.SignedAt,
            ContractPdfUrl = contract.ContractPdfUrl,
            CreatedAt = contract.CreatedAt
        };
    }

    private string ReplaceContractPlaceholders(
        string template,
        User buyer,
        User seller,
        Post post,
        decimal price)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        if (buyer == null)
            throw new ArgumentNullException(nameof(buyer), "Buyer cannot be null");
        
        if (seller == null)
            throw new ArgumentNullException(nameof(seller), "Seller cannot be null");
        
        if (post == null)
            throw new ArgumentNullException(nameof(post), "Post cannot be null");

        return template
            .Replace("{{BUYER_NAME}}", buyer.FullName ?? buyer.Email ?? "")
            .Replace("{{BUYER_EMAIL}}", buyer.Email ?? "")
            .Replace("{{BUYER_PHONE}}", buyer.PhoneNumber ?? "")
            .Replace("{{BUYER_ADDRESS}}", buyer.Address ?? "")
            .Replace("{{SELLER_NAME}}", seller.FullName ?? seller.Email ?? "")
            .Replace("{{SELLER_EMAIL}}", seller.Email ?? "")
            .Replace("{{SELLER_PHONE}}", seller.PhoneNumber ?? "")
            .Replace("{{SELLER_ADDRESS}}", seller.Address ?? "")
            .Replace("{{PRODUCT_TITLE}}", post.Title ?? "")
            .Replace("{{PRODUCT_DESCRIPTION}}", post.Description ?? "")
            .Replace("{{PRODUCT_BRAND}}", post.Brand ?? "")
            .Replace("{{PRODUCT_MODEL}}", post.Model ?? "")
            .Replace("{{PRODUCT_PRICE}}", price.ToString("N0"))
            .Replace("{{CONTRACT_DATE}}", DateTime.UtcNow.ToString("dd/MM/yyyy"));
    }

    /// <summary>
    /// UC43: Lấy danh sách hợp đồng của Staff
    /// </summary>
    public async Task<BaseResponse<PagedResponse<ContractResponse>>> GetContractsByStaffIdAsync(
        Guid staffId,
        ContractSearchRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<PagedResponse<ContractResponse>>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Validate request
            request.IsValid();

            // 3. Get contracts from repository
            var contracts = await _contractRepository.GetContractsByStaffIdAsync(
                staffId,
                request.Status,
                request.LeadId,
                request.OrderId);

            // 4. Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                contracts = request.SortBy.ToLower() switch
                {
                    "createdat" => request.SortOrder?.ToUpper() == "ASC"
                        ? contracts.OrderBy(c => c.CreatedAt).ToList()
                        : contracts.OrderByDescending(c => c.CreatedAt).ToList(),
                    "status" => request.SortOrder?.ToUpper() == "ASC"
                        ? contracts.OrderBy(c => c.Status).ToList()
                        : contracts.OrderByDescending(c => c.Status).ToList(),
                    "signedat" => request.SortOrder?.ToUpper() == "ASC"
                        ? contracts.OrderBy(c => c.SignedAt ?? DateTime.MaxValue).ToList()
                        : contracts.OrderByDescending(c => c.SignedAt ?? DateTime.MinValue).ToList(),
                    _ => contracts
                };
            }

            // 5. Apply pagination
            var totalCount = contracts.Count;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var skip = (pageNumber - 1) * pageSize;
            var pagedContracts = contracts.Skip(skip).Take(pageSize).ToList();

            // 6. Map to response
            var responses = pagedContracts.Select(MapToResponse).ToList();

            // 7. Create paged response
            var pagedResponse = PagedResponse<ContractResponse>.SuccessResponse(
                responses,
                pageNumber,
                pageSize,
                totalCount,
                "Lấy danh sách hợp đồng thành công");

            return BaseResponse<PagedResponse<ContractResponse>>.SuccessResponse(
                pagedResponse,
                "Lấy danh sách hợp đồng thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách hợp đồng, StaffId: {StaffId}", staffId);
            return BaseResponse<PagedResponse<ContractResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách hợp đồng");
        }
    }

    /// <summary>
    /// UC43: Gửi hợp đồng để ký
    /// </summary>
    public async Task<BaseResponse<ContractResponse>> SendContractForSignatureAsync(
        Guid staffId,
        Guid contractId)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get contract with details
            var contract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (contract == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng");
            }

            // 3. Validate contract belongs to this Staff
            if (contract.CreatedBy != staffId)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Bạn không có quyền gửi hợp đồng này");
            }

            // 4. Validate contract status
            if (contract.Status != "DRAFT")
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Chỉ có thể gửi hợp đồng ở trạng thái DRAFT");
            }

            // 5. Ensure PDF is generated
            if (string.IsNullOrEmpty(contract.ContractPdfUrl))
            {
                try
                {
                    var pdfUrl = await GenerateContractPdfAsync(contractId, contract.ContractContent ?? string.Empty);
                    contract.ContractPdfUrl = pdfUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể tạo PDF cho hợp đồng {ContractId}", contractId);
                    return BaseResponse<ContractResponse>.FailureResponse(
                        "Không thể tạo file PDF hợp đồng. Vui lòng thử lại sau.");
                }
            }

            // 6. Update contract status
            contract.Status = "PENDING_SIGNATURE";
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            // 7. Create notifications for buyer and seller
            if (contract.OrderId.HasValue && contract.Order != null)
            {
                var buyerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = contract.Order.BuyerId,
                    NotificationType = "CONTRACT_PENDING_SIGNATURE",
                    Title = "Hợp đồng cần ký",
                    Content = $"Hợp đồng mua bán đã được gửi. Vui lòng xem xét và ký hợp đồng.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(buyerNotification);

                var sellerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = contract.Order.SellerId,
                    NotificationType = "CONTRACT_PENDING_SIGNATURE",
                    Title = "Hợp đồng cần ký",
                    Content = $"Hợp đồng mua bán đã được gửi. Vui lòng xem xét và ký hợp đồng.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(sellerNotification);
            }
            else if (contract.LeadId.HasValue && contract.Lead != null)
            {
                var buyerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = contract.Lead.BuyerId,
                    NotificationType = "CONTRACT_PENDING_SIGNATURE",
                    Title = "Hợp đồng cần ký",
                    Content = $"Hợp đồng mua bán đã được gửi. Vui lòng xem xét và ký hợp đồng.",
                    RelatedId = contract.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(buyerNotification);

                if (contract.Lead.Post != null)
                {
                    var sellerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = contract.Lead.Post.UserId,
                        NotificationType = "CONTRACT_PENDING_SIGNATURE",
                        Title = "Hợp đồng cần ký",
                        Content = $"Hợp đồng mua bán đã được gửi. Vui lòng xem xét và ký hợp đồng.",
                        RelatedId = contract.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(sellerNotification);
                }
            }

            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã gửi hợp đồng {ContractId} để ký",
                staffId,
                contractId);

            // 8. Get updated contract with details
            var updatedContract = await _contractRepository.GetByIdWithDetailsAsync(contractId);
            if (updatedContract == null)
            {
                return BaseResponse<ContractResponse>.FailureResponse(
                    "Không tìm thấy hợp đồng sau khi cập nhật");
            }

            // 9. Map to response
            var response = MapToResponse(updatedContract);

            return BaseResponse<ContractResponse>.SuccessResponse(
                response,
                "Gửi hợp đồng để ký thành công. Đã thông báo cho cả hai bên.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi hợp đồng để ký, StaffId: {StaffId}, ContractId: {ContractId}", staffId, contractId);
            return BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gửi hợp đồng để ký");
        }
    }

    private async Task<string> GenerateContractPdfAsync(Guid contractId, string content)
    {
        // Mock implementation - in real scenario, use a PDF library like iTextSharp, QuestPDF, etc.
        // For now, return a placeholder URL
        await Task.CompletedTask;
        return $"/contracts/{contractId}/download.pdf";
    }
}

