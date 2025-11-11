using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Leads;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Lead Service implementation
/// </summary>
public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPostStaffAssignmentRepository _postStaffAssignmentRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        ILeadRepository leadRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IPostStaffAssignmentRepository postStaffAssignmentRepository,
        IChatRepository chatRepository,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _postStaffAssignmentRepository = postStaffAssignmentRepository;
        _chatRepository = chatRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<LeadResponse>> CreateLeadAsync(
        Guid userId,
        LeadCreateRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Validate post exists and is approved
            // Load post with staff assignments để kiểm tra xem đã có Staff chưa
            var post = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
            if (post == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            if (post.Status != "APPROVED")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Bài đăng chưa được duyệt, không thể tạo Lead");
            }

            // 3. Validate post is not sold
            if (post.IsSold)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Sản phẩm đã được bán");
            }

            // 4. Validate user is not the seller
            if (post.UserId == userId)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Bạn không thể tạo Lead cho sản phẩm của chính mình");
            }

            // 5. Check if lead already exists for this post and buyer
            var existingLead = await _leadRepository.GetByPostIdAndBuyerIdAsync(
                request.PostId,
                userId);

            if (existingLead != null)
            {
                // Nếu Lead đã tồn tại và ở trạng thái NEW hoặc ASSIGNED, không tạo mới
                if (existingLead.Status == "NEW" || existingLead.Status == "ASSIGNED")
                {
                    string message = existingLead.Status == "ASSIGNED"
                        ? "Bạn đã tạo Lead cho sản phẩm này."
                        : "Bạn đã tạo Lead cho sản phẩm này.";
                    
                    return BaseResponse<LeadResponse>.FailureResponse(message);
                }
            }

            // 6. Validate LeadType
            if (request.LeadType != "SCHEDULE_VIEW" && request.LeadType != "AUCTION_WINNER")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Loại Lead không hợp lệ");
            }

            // 7. Kiểm tra xem Post đã có Staff được gán chưa
            Guid? assignedStaffId = null;
            Guid? assignedByAdminId = null;
            string leadStatus = "NEW";
            DateTime? assignedAt = null;

            // Tìm Staff assignment active cho Post này
            var activeStaffAssignment = post.PostStaffAssignments
                .FirstOrDefault(psa => psa.IsActive);

            if (activeStaffAssignment != null)
            {
                // Nếu Post đã có Staff, tự động gán Staff đó cho Lead
                assignedStaffId = activeStaffAssignment.StaffId;
                assignedByAdminId = activeStaffAssignment.AssignedBy;
                leadStatus = "ASSIGNED";
                assignedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Tự động gán Staff {StaffId} cho Lead từ Post {PostId} (Staff đã được gán bởi Admin {AdminId})",
                    assignedStaffId,
                    request.PostId,
                    assignedByAdminId);
            }
            else
            {
                // Nếu Post chưa có Staff, tự động gán Staff cho Post và Lead
                // Lấy danh sách staff và chọn staff đầu tiên (có thể cải thiện bằng round-robin hoặc workload)
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
                        "Tự động gán Staff {StaffId} cho Post {PostId} và Lead (khi có người yêu cầu tư vấn), AssignedBy: {AdminId}",
                        assignedStaffId,
                        request.PostId,
                        assignedByAdminId);
                }
                else
                {
                    // Không có staff hoặc admin available, để Lead ở trạng thái NEW và Admin sẽ gán sau
                    _logger.LogWarning(
                        "Không có Staff hoặc Admin available để gán cho Post {PostId}. Lead sẽ ở trạng thái NEW và Admin sẽ gán sau.",
                        request.PostId);
                }
            }

            // 8. Create Lead
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                BuyerId = userId,
                StaffId = assignedStaffId, // Tự động gán nếu Post đã có Staff hoặc đã gán mới
                AssignedBy = assignedByAdminId, // Admin đã gán Staff (có thể là admin đầu tiên nếu tự động gán)
                LeadType = request.LeadType,
                Status = leadStatus, // ASSIGNED nếu đã có Staff, NEW nếu chưa có
                FinalPrice = null, // Sẽ được set khi là AUCTION_WINNER
                AssignedAt = assignedAt, // Set thời gian nếu đã gán Staff
                ClosedAt = null,
                Notes = null,
                CreatedAt = DateTime.UtcNow
            };

            await _leadRepository.CreateAsync(lead);
            await _leadRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Người dùng {UserId} đã tạo Lead {LeadId} cho bài đăng {PostId}, Loại: {LeadType}, Status: {Status}, StaffId: {StaffId}",
                userId,
                lead.Id,
                request.PostId,
                request.LeadType,
                lead.Status,
                lead.StaffId);

            // 9. Create notifications
            if (assignedStaffId.HasValue)
            {
                // Nếu đã có Staff, chỉ gửi notification cho Staff đó
                var staffNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = assignedStaffId.Value,
                    NotificationType = "NEW_LEAD_ASSIGNED",
                    Title = "Có Lead mới được gán cho bạn",
                    Content = $"Bạn đã được gán Lead mới cho bài đăng \"{post.Title}\". Loại: {request.LeadType}",
                    RelatedId = lead.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(staffNotification);
                await _notificationRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Đã gửi notification cho Staff {StaffId} về Lead {LeadId}",
                    assignedStaffId.Value,
                    lead.Id);
            }
            else
            {
                // Nếu chưa có Staff, gửi notification cho tất cả Admins để gán Staff
                var admins = await _userRepository.GetAdminsAsync();
                foreach (var admin in admins)
                {
                    var adminNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = admin.Id,
                        NotificationType = "NEW_LEAD",
                        Title = "Có Lead mới cần gán Staff",
                        Content = $"Có người đã tạo Lead cho bài đăng \"{post.Title}\". Loại: {request.LeadType}. Vui lòng gán Staff.",
                        RelatedId = lead.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(adminNotification);
                }
                await _notificationRepository.SaveChangesAsync();
            }

            // 10. Get lead with details for response
            var leadWithDetails = await _leadRepository.GetByIdWithDetailsAsync(lead.Id);
            if (leadWithDetails == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead vừa tạo");
            }

            // 11. Map to response
            var response = MapToResponse(leadWithDetails);

            // 12. Return success message based on whether Staff was assigned
            string successMessage = assignedStaffId.HasValue
                ? "Tạo Lead thành công. Staff đã được tự động gán cho bài đăng và sẽ liên hệ với bạn sớm."
                : "Tạo Lead thành công. Admin sẽ sớm gán Staff để hỗ trợ bạn.";

            return BaseResponse<LeadResponse>.SuccessResponse(
                response,
                successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Lead, UserId: {UserId}, PostId: {PostId}", userId, request.PostId);
            return BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo Lead");
        }
    }

    /// <summary>
    /// UC40: Lấy danh sách Lead được gán cho Staff
    /// </summary>
    public async Task<BaseResponse<PagedResponse<LeadResponse>>> GetLeadsByStaffIdAsync(
        Guid staffId,
        LeadSearchRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Use staffId from parameter, not from request (security)
            var leads = await _leadRepository.GetLeadsByStaffIdAsync(
                staffId,
                request.Status,
                request.LeadType,
                request.PostId,
                request.BuyerId);

            // 3. Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                leads = request.SortBy.ToLower() switch
                {
                    "createdat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.CreatedAt).ToList()
                        : leads.OrderByDescending(l => l.CreatedAt).ToList(),
                    "assignedat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.Where(l => l.AssignedAt.HasValue).OrderBy(l => l.AssignedAt).ToList()
                        : leads.Where(l => l.AssignedAt.HasValue).OrderByDescending(l => l.AssignedAt).ToList(),
                    "status" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.Status).ToList()
                        : leads.OrderByDescending(l => l.Status).ToList(),
                    _ => leads
                };
            }

            // 4. Apply pagination
            var totalCount = leads.Count;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var skip = (pageNumber - 1) * pageSize;
            var pagedLeads = leads.Skip(skip).Take(pageSize).ToList();

            // 5. Map to response
            var responses = pagedLeads.Select(MapToResponse).ToList();

            // 6. Create paged response
            var pagedResponse = PagedResponse<LeadResponse>.SuccessResponse(
                responses,
                pageNumber,
                pageSize,
                totalCount,
                "Lấy danh sách Lead thành công");

            return BaseResponse<PagedResponse<LeadResponse>>.SuccessResponse(
                pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Leads, StaffId: {StaffId}", staffId);
            return BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Leads");
        }
    }

    /// <summary>
    /// Lấy chi tiết Lead
    /// </summary>
    public async Task<BaseResponse<LeadResponse>> GetLeadByIdAsync(
        Guid leadId,
        Guid? staffId = null)
    {
        try
        {
            // 1. Get lead with details
            var lead = await _leadRepository.GetByIdWithDetailsAsync(leadId);
            if (lead == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead");
            }

            // 2. If staffId is provided, validate that the lead belongs to this Staff
            if (staffId.HasValue && lead.StaffId != staffId.Value)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Bạn không có quyền xem Lead này");
            }

            // 3. Map to response
            var response = MapToResponse(lead);

            return BaseResponse<LeadResponse>.SuccessResponse(
                response,
                "Lấy chi tiết Lead thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết Lead, LeadId: {LeadId}", leadId);
            return BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết Lead");
        }
    }

    /// <summary>
    /// UC44: Cập nhật trạng thái Lead
    /// </summary>
    public async Task<BaseResponse<LeadResponse>> UpdateLeadStatusAsync(
        Guid staffId,
        Guid leadId,
        LeadStatusUpdateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get Lead with details (có tracking để update)
            var lead = await _leadRepository.GetByIdWithDetailsForUpdateAsync(leadId);
            if (lead == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead");
            }

            // 3. Validate Lead belongs to this Staff
            if (lead.StaffId != staffId)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Bạn không có quyền cập nhật Lead này");
            }

            // 4. Validate status transition
            var validStatuses = new[] { "CONTACTED", "SCHEDULED", "SUCCESSFUL", "FAILED" };
            var newStatus = request.Status.ToUpper();
            if (!validStatuses.Contains(newStatus))
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Trạng thái không hợp lệ. Chỉ chấp nhận: CONTACTED, SCHEDULED, SUCCESSFUL, FAILED");
            }

            // 5. Update status and notes
            var oldStatus = lead.Status;
            lead.Status = newStatus;
            
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                lead.Notes = request.Notes;
            }

            // 6. If status is SUCCESSFUL or FAILED, set ClosedAt
            if (newStatus == "SUCCESSFUL" || newStatus == "FAILED")
            {
                lead.ClosedAt = DateTime.UtcNow;
            }

            // 7. If status is SUCCESSFUL, update post is_sold = true
            if (newStatus == "SUCCESSFUL")
            {
                // Sử dụng Post từ Lead.Post (đã được load và tracked) hoặc load mới nếu chưa có
                var post = lead.Post ?? await _postRepository.GetByIdForUpdateAsync(lead.PostId);
                if (post != null)
                {
                    post.IsSold = true;
                    await _postRepository.UpdateAsync(post);
                    await _postRepository.SaveChangesAsync();

                    _logger.LogInformation(
                        "Đã cập nhật is_sold = true cho bài đăng {PostId} khi Lead {LeadId} thành công",
                        lead.PostId,
                        leadId);
                }
            }

            // 8. Save changes
            await _leadRepository.UpdateAsync(lead);
            await _leadRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã cập nhật trạng thái Lead {LeadId} từ {OldStatus} sang {NewStatus}",
                staffId,
                leadId,
                oldStatus,
                newStatus);

            // 9. Create notifications
            if (newStatus == "SUCCESSFUL")
            {
                // Notify buyer
                var buyerNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = lead.BuyerId,
                    NotificationType = "LEAD_SUCCESSFUL",
                    Title = "Giao dịch thành công",
                    Content = $"Giao dịch cho bài đăng \"{lead.Post?.Title}\" đã thành công. Hệ thống sẽ tạo đơn hàng.",
                    RelatedId = lead.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.CreateAsync(buyerNotification);

                // Notify seller
                if (lead.Post != null)
                {
                    var sellerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = lead.Post.UserId,
                        NotificationType = "LEAD_SUCCESSFUL",
                        Title = "Giao dịch thành công",
                        Content = $"Giao dịch cho bài đăng \"{lead.Post.Title}\" đã thành công.",
                        RelatedId = lead.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.CreateAsync(sellerNotification);
                }

                await _notificationRepository.SaveChangesAsync();
            }

            // 10. Get updated lead with details
            var updatedLead = await _leadRepository.GetByIdWithDetailsAsync(leadId);
            if (updatedLead == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead sau khi cập nhật");
            }

            // 11. Map to response
            var response = MapToResponse(updatedLead);

            return BaseResponse<LeadResponse>.SuccessResponse(
                response,
                "Cập nhật trạng thái Lead thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Lead, StaffId: {StaffId}, LeadId: {LeadId}", staffId, leadId);
            return BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật trạng thái Lead");
        }
    }

    /// <summary>
    /// UC46: Admin gán Staff cho Lead
    /// </summary>
    public async Task<BaseResponse<LeadResponse>> AssignStaffToLeadAsync(
        Guid adminId,
        Guid leadId,
        LeadAssignStaffRequest request)
    {
        try
        {
            // 1. Validate Admin exists
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null || admin.Role != "ADMIN")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Admin không tồn tại hoặc không có quyền");
            }

            // 2. Validate Lead exists and get with details (with tracking for update)
            var lead = await _leadRepository.GetByIdWithDetailsForUpdateAsync(leadId);
            if (lead == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead");
            }

            // 3. Validate Lead status is NEW (chỉ gán Staff cho Lead mới)
            if (lead.Status != "NEW")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    $"Không thể gán Staff cho Lead đang ở trạng thái {lead.Status}. Chỉ có thể gán Staff cho Lead có trạng thái NEW.");
            }

            // 4. Validate Staff exists and is STAFF role
            var staff = await _userRepository.GetByIdAsync(request.StaffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 5. Validate Staff status is ACTIVE
            if (staff.Status != "ACTIVE")
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Staff không ở trạng thái ACTIVE, không thể gán Lead");
            }

            // 6. Validate Post is loaded (should be loaded via Include)
            if (lead.Post == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy bài đăng liên quan");
            }

            var post = lead.Post;
            var sellerId = post.UserId;

            // 7. Update Lead: staff_id, assigned_by, assigned_at, status = ASSIGNED
            lead.StaffId = request.StaffId;
            lead.AssignedBy = adminId;
            lead.AssignedAt = DateTime.UtcNow;
            lead.Status = "ASSIGNED";

            await _leadRepository.UpdateAsync(lead);
            await _leadRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {AdminId} đã gán Staff {StaffId} cho Lead {LeadId}",
                adminId,
                request.StaffId,
                leadId);

            // 8. Tạo hoặc cập nhật phòng chat 3 người (Buyer, Seller, Staff) nếu chưa có
            var existingChatRoom = await _chatRepository.GetRoomByLeadIdAsync(leadId);
            
            if (existingChatRoom == null)
            {
                // Tạo phòng chat mới với 3 người: Buyer, Seller, Staff
                var chatRoom = new ChatRoom
                {
                    Id = Guid.NewGuid(),
                    LeadId = leadId,
                    PostId = lead.PostId,
                    BuyerId = lead.BuyerId,
                    SellerId = sellerId,
                    StaffId = request.StaffId,
                    LastMessageAt = null,
                    CreatedAt = DateTime.UtcNow
                };

                await _chatRepository.CreateRoomAsync(chatRoom);
                await _chatRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Đã tạo phòng chat {RoomId} cho Lead {LeadId} với Buyer {BuyerId}, Seller {SellerId}, Staff {StaffId}",
                    chatRoom.Id,
                    leadId,
                    lead.BuyerId,
                    sellerId,
                    request.StaffId);
            }
            else
            {
                // Nếu đã có phòng chat, cập nhật StaffId nếu chưa có
                if (existingChatRoom.StaffId == null)
                {
                    existingChatRoom.StaffId = request.StaffId;
                    await _chatRepository.UpdateRoomAsync(existingChatRoom);
                    await _chatRepository.SaveChangesAsync();

                    _logger.LogInformation(
                        "Đã cập nhật phòng chat {RoomId} với Staff {StaffId} cho Lead {LeadId}",
                        existingChatRoom.Id,
                        request.StaffId,
                        leadId);
                }
            }

            // 9. Gửi thông báo cho Staff về Lead mới được gán
            var staffNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.StaffId,
                NotificationType = "LEAD_ASSIGNED",
                Title = "Bạn đã được gán Lead mới",
                Content = $"Bạn đã được Admin gán Lead mới cho bài đăng \"{post.Title}\". Loại: {lead.LeadType}",
                RelatedId = leadId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(staffNotification);
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Đã gửi notification cho Staff {StaffId} về Lead {LeadId}",
                request.StaffId,
                leadId);

            // 10. Get updated lead with details
            var updatedLead = await _leadRepository.GetByIdWithDetailsAsync(leadId);
            if (updatedLead == null)
            {
                return BaseResponse<LeadResponse>.FailureResponse(
                    "Không tìm thấy Lead sau khi cập nhật");
            }

            // 11. Map to response
            var response = MapToResponse(updatedLead);

            return BaseResponse<LeadResponse>.SuccessResponse(
                response,
                "Gán Staff cho Lead thành công. Phòng chat đã được tạo và Staff đã nhận được thông báo.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gán Staff cho Lead, AdminId: {AdminId}, LeadId: {LeadId}, StaffId: {StaffId}", 
                adminId, leadId, request.StaffId);
            return BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gán Staff cho Lead");
        }
    }

    /// <summary>
    /// UC23: Lấy danh sách Leads của Member (người mua)
    /// </summary>
    public async Task<BaseResponse<PagedResponse<LeadResponse>>> GetMyLeadsAsync(
        Guid buyerId,
        LeadSearchRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(buyerId);
            if (user == null)
            {
                return BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Get leads by buyerId
            var leads = await _leadRepository.GetLeadsByBuyerIdAsync(buyerId);

            // 3. Apply filters
            if (!string.IsNullOrEmpty(request.Status))
            {
                leads = leads.Where(l => l.Status == request.Status.ToUpper()).ToList();
            }

            if (!string.IsNullOrEmpty(request.LeadType))
            {
                leads = leads.Where(l => l.LeadType == request.LeadType.ToUpper()).ToList();
            }

            if (request.PostId.HasValue)
            {
                leads = leads.Where(l => l.PostId == request.PostId.Value).ToList();
            }

            // 4. Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                leads = request.SortBy.ToLower() switch
                {
                    "createdat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.CreatedAt).ToList()
                        : leads.OrderByDescending(l => l.CreatedAt).ToList(),
                    "assignedat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.Where(l => l.AssignedAt.HasValue).OrderBy(l => l.AssignedAt).ToList()
                        : leads.Where(l => l.AssignedAt.HasValue).OrderByDescending(l => l.AssignedAt).ToList(),
                    "status" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.Status).ToList()
                        : leads.OrderByDescending(l => l.Status).ToList(),
                    _ => leads
                };
            }

            // 5. Apply pagination
            var totalCount = leads.Count;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var skip = (pageNumber - 1) * pageSize;
            var pagedLeads = leads.Skip(skip).Take(pageSize).ToList();

            // 6. Map to response
            var responses = pagedLeads.Select(MapToResponse).ToList();

            // 7. Create paged response
            var pagedResponse = PagedResponse<LeadResponse>.SuccessResponse(
                responses,
                pageNumber,
                pageSize,
                totalCount,
                "Lấy danh sách Lead thành công");

            return BaseResponse<PagedResponse<LeadResponse>>.SuccessResponse(
                pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Leads của Member, BuyerId: {BuyerId}", buyerId);
            return BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Leads");
        }
    }

    /// <summary>
    /// UC46: Lấy danh sách tất cả Leads (dành cho Admin)
    /// </summary>
    public async Task<PagedResponse<LeadResponse>> GetAllLeadsAsync(
        LeadSearchRequest request)
    {
        try
        {
            // 1. Get all leads with filters
            var leads = await _leadRepository.GetAllLeadsAsync(
                request.Status,
                request.LeadType,
                request.PostId,
                request.BuyerId,
                request.StaffId);

            // 2. Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                leads = request.SortBy.ToLower() switch
                {
                    "createdat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.CreatedAt).ToList()
                        : leads.OrderByDescending(l => l.CreatedAt).ToList(),
                    "assignedat" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.Where(l => l.AssignedAt.HasValue).OrderBy(l => l.AssignedAt).ToList()
                        : leads.Where(l => l.AssignedAt.HasValue).OrderByDescending(l => l.AssignedAt).ToList(),
                    "status" => request.SortOrder?.ToUpper() == "ASC"
                        ? leads.OrderBy(l => l.Status).ToList()
                        : leads.OrderByDescending(l => l.Status).ToList(),
                    _ => leads
                };
            }

            // 3. Apply pagination
            var totalCount = leads.Count;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var skip = (pageNumber - 1) * pageSize;
            var pagedLeads = leads.Skip(skip).Take(pageSize).ToList();

            // 4. Map to response
            var responses = pagedLeads.Select(MapToResponse).ToList();

            // 5. Create paged response
            return PagedResponse<LeadResponse>.SuccessResponse(
                responses,
                pageNumber,
                pageSize,
                totalCount,
                "Lấy danh sách Lead thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tất cả Leads");
            return PagedResponse<LeadResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách Leads");
        }
    }

    private LeadResponse MapToResponse(Lead lead)
    {
        return new LeadResponse
        {
            LeadId = lead.Id,
            PostId = lead.PostId,
            PostTitle = lead.Post?.Title ?? string.Empty,
            BuyerId = lead.BuyerId,
            BuyerName = lead.Buyer?.FullName ?? lead.Buyer?.Email ?? string.Empty,
            BuyerEmail = lead.Buyer?.Email ?? string.Empty,
            BuyerPhone = lead.Buyer?.PhoneNumber,
            BuyerAddress = lead.Buyer?.Address,
            // Seller info from Post.User
            SellerId = lead.Post?.UserId,
            SellerName = lead.Post?.User?.FullName ?? lead.Post?.User?.Email,
            SellerEmail = lead.Post?.User?.Email,
            SellerPhone = lead.Post?.User?.PhoneNumber,
            SellerAddress = lead.Post?.User?.Address,
            // Post info
            PostBrand = lead.Post?.Brand,
            PostModel = lead.Post?.Model,
            PostDescription = lead.Post?.Description,
            PostPrice = lead.Post?.Price,
            StaffId = lead.StaffId,
            StaffName = lead.Staff?.FullName ?? lead.Staff?.Email,
            AssignedBy = lead.AssignedBy,
            LeadType = lead.LeadType,
            Status = lead.Status,
            FinalPrice = lead.FinalPrice,
            AssignedAt = lead.AssignedAt,
            ClosedAt = lead.ClosedAt,
            Notes = lead.Notes,
            CreatedAt = lead.CreatedAt
        };
    }
}

