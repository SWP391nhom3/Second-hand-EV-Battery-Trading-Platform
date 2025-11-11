using EVehicle.Application.DTOs.Appointments;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Appointment Service implementation
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ILeadRepository leadRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _leadRepository = leadRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// UC41: Tạo Appointment (Tạo Lịch hẹn)
    /// </summary>
    public async Task<BaseResponse<AppointmentResponse>> CreateAppointmentAsync(
        Guid staffId,
        AppointmentCreateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Validate Lead exists and belongs to this Staff
            var lead = await _leadRepository.GetByIdWithDetailsAsync(request.LeadId);
            if (lead == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy Lead");
            }

            if (lead.StaffId != staffId)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Bạn không có quyền tạo Appointment cho Lead này");
            }

            // 3. Validate Post exists
            var post = await _postRepository.GetByIdWithDetailsAsync(lead.PostId);
            if (post == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy bài đăng");
            }

            // 4. Get Seller from Post
            var seller = await _userRepository.GetByIdAsync(post.UserId);
            if (seller == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy người bán");
            }

            // 5. Get Buyer
            var buyer = await _userRepository.GetByIdAsync(lead.BuyerId);
            if (buyer == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy người mua");
            }

            // 6. Validate StartTime is in the future
            if (request.StartTime <= DateTime.UtcNow)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Thời gian bắt đầu phải trong tương lai");
            }

            // 7. Validate EndTime is after StartTime
            if (request.EndTime.HasValue && request.EndTime.Value <= request.StartTime)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Thời gian kết thúc phải sau thời gian bắt đầu");
            }

            // 8. Create Appointment
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                LeadId = request.LeadId,
                PostId = lead.PostId,
                BuyerId = lead.BuyerId,
                SellerId = post.UserId,
                StaffId = staffId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location,
                Notes = request.Notes,
                Status = "CONFIRMED",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _appointmentRepository.CreateAsync(appointment);

            // 9. Update Lead status to SCHEDULED
            lead.Status = "SCHEDULED";
            await _leadRepository.UpdateAsync(lead);
            await _leadRepository.SaveChangesAsync();

            // 10. Save Appointment
            await _appointmentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã tạo Appointment {AppointmentId} cho Lead {LeadId}",
                staffId,
                appointment.Id,
                request.LeadId);

            // 11. Create notifications for Buyer, Seller, and Staff
            var buyerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = buyer.Id,
                NotificationType = "APPOINTMENT_CREATED",
                Title = "Lịch hẹn đã được tạo",
                Content = $"Lịch hẹn xem sản phẩm \"{post.Title}\" đã được tạo. Thời gian: {request.StartTime:dd/MM/yyyy HH:mm} tại {request.Location}",
                RelatedId = appointment.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = seller.Id,
                NotificationType = "APPOINTMENT_CREATED",
                Title = "Lịch hẹn đã được tạo",
                Content = $"Lịch hẹn xem sản phẩm \"{post.Title}\" đã được tạo. Thời gian: {request.StartTime:dd/MM/yyyy HH:mm} tại {request.Location}",
                RelatedId = appointment.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var staffNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = staffId,
                NotificationType = "APPOINTMENT_CREATED",
                Title = "Lịch hẹn đã được tạo",
                Content = $"Bạn đã tạo lịch hẹn xem sản phẩm \"{post.Title}\". Thời gian: {request.StartTime:dd/MM/yyyy HH:mm} tại {request.Location}",
                RelatedId = appointment.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(buyerNotification);
            await _notificationRepository.CreateAsync(sellerNotification);
            await _notificationRepository.CreateAsync(staffNotification);
            await _notificationRepository.SaveChangesAsync();

            // 12. Get appointment with details for response
            var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id);
            if (appointmentWithDetails == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy Appointment vừa tạo");
            }

            // 13. Map to response
            var response = MapToResponse(appointmentWithDetails);

            return BaseResponse<AppointmentResponse>.SuccessResponse(
                response,
                "Tạo lịch hẹn thành công. Thông báo đã được gửi cho cả ba bên.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Appointment, StaffId: {StaffId}, LeadId: {LeadId}", staffId, request.LeadId);
            return BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo Appointment");
        }
    }

    /// <summary>
    /// UC42: Lấy danh sách Appointments (Quản lý Lịch hẹn)
    /// </summary>
    public async Task<BaseResponse<PagedResponse<AppointmentResponse>>> GetAppointmentsAsync(
        Guid staffId,
        AppointmentSearchRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<PagedResponse<AppointmentResponse>>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Use staffId from parameter, not from request (security)
            var appointments = await _appointmentRepository.GetAppointmentsByStaffIdAsync(
                staffId,
                request.Status,
                request.Upcoming,
                request.Past,
                request.LeadId,
                request.PostId);

            // 3. Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                appointments = request.SortBy.ToLower() switch
                {
                    "starttime" => request.SortOrder?.ToUpper() == "DESC"
                        ? appointments.OrderByDescending(a => a.StartTime).ToList()
                        : appointments.OrderBy(a => a.StartTime).ToList(),
                    "createdat" => request.SortOrder?.ToUpper() == "DESC"
                        ? appointments.OrderByDescending(a => a.CreatedAt).ToList()
                        : appointments.OrderBy(a => a.CreatedAt).ToList(),
                    _ => appointments
                };
            }

            // 4. Apply pagination
            var totalCount = appointments.Count;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var skip = (pageNumber - 1) * pageSize;
            var pagedAppointments = appointments.Skip(skip).Take(pageSize).ToList();

            // 5. Map to response
            var responses = pagedAppointments.Select(MapToResponse).ToList();

            // 6. Create paged response
            var pagedResponse = PagedResponse<AppointmentResponse>.SuccessResponse(
                responses,
                pageNumber,
                pageSize,
                totalCount,
                "Lấy danh sách lịch hẹn thành công");

            return BaseResponse<PagedResponse<AppointmentResponse>>.SuccessResponse(
                pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Appointments, StaffId: {StaffId}", staffId);
            return BaseResponse<PagedResponse<AppointmentResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Appointments");
        }
    }

    /// <summary>
    /// UC42: Lấy chi tiết Appointment
    /// </summary>
    public async Task<BaseResponse<AppointmentResponse>> GetAppointmentByIdAsync(
        Guid appointmentId,
        Guid staffId)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get appointment with details
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointment == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy lịch hẹn");
            }

            // 3. Validate appointment belongs to this Staff
            if (appointment.StaffId != staffId)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Bạn không có quyền xem lịch hẹn này");
            }

            // 4. Map to response
            var response = MapToResponse(appointment);

            return BaseResponse<AppointmentResponse>.SuccessResponse(
                response,
                "Lấy chi tiết lịch hẹn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết Appointment, AppointmentId: {AppointmentId}, StaffId: {StaffId}", appointmentId, staffId);
            return BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết Appointment");
        }
    }

    /// <summary>
    /// UC42: Cập nhật Appointment
    /// </summary>
    public async Task<BaseResponse<AppointmentResponse>> UpdateAppointmentAsync(
        Guid appointmentId,
        Guid staffId,
        AppointmentUpdateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get appointment
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy lịch hẹn");
            }

            // 3. Validate appointment belongs to this Staff
            if (appointment.StaffId != staffId)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Bạn không có quyền cập nhật lịch hẹn này");
            }

            // 4. Validate appointment is not canceled or completed
            if (appointment.Status == "CANCELED" || appointment.Status == "COMPLETED")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể cập nhật lịch hẹn đã bị hủy hoặc đã hoàn thành");
            }

            // 5. Update fields
            if (request.StartTime.HasValue)
            {
                if (request.StartTime.Value <= DateTime.UtcNow)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Thời gian bắt đầu phải trong tương lai");
                }
                appointment.StartTime = request.StartTime.Value;
            }

            if (request.EndTime.HasValue)
            {
                if (request.EndTime.Value <= appointment.StartTime)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Thời gian kết thúc phải sau thời gian bắt đầu");
                }
                appointment.EndTime = request.EndTime;
            }

            if (!string.IsNullOrEmpty(request.Location))
            {
                appointment.Location = request.Location;
            }

            if (request.Notes != null)
            {
                appointment.Notes = request.Notes;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var validStatuses = new[] { "CONFIRMED", "CANCELED", "COMPLETED" };
                if (!validStatuses.Contains(request.Status.ToUpper()))
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Trạng thái không hợp lệ");
                }
                appointment.Status = request.Status.ToUpper();
            }

            appointment.UpdatedAt = DateTime.UtcNow;

            // 6. Save changes
            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã cập nhật Appointment {AppointmentId}",
                staffId,
                appointmentId);

            // 7. Get appointment with details for response
            var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointmentWithDetails == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy Appointment vừa cập nhật");
            }

            // 8. Send notifications if status changed
            if (!string.IsNullOrEmpty(request.Status) && request.Status != appointment.Status)
            {
                var appointmentDetail = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointmentDetail != null)
                {
                    var buyerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = appointmentDetail.BuyerId,
                        NotificationType = "APPOINTMENT_UPDATED",
                        Title = "Lịch hẹn đã được cập nhật",
                        Content = $"Lịch hẹn đã được cập nhật. Trạng thái: {request.Status}",
                        RelatedId = appointmentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var sellerNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = appointmentDetail.SellerId,
                        NotificationType = "APPOINTMENT_UPDATED",
                        Title = "Lịch hẹn đã được cập nhật",
                        Content = $"Lịch hẹn đã được cập nhật. Trạng thái: {request.Status}",
                        RelatedId = appointmentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _notificationRepository.CreateAsync(buyerNotification);
                    await _notificationRepository.CreateAsync(sellerNotification);
                    await _notificationRepository.SaveChangesAsync();
                }
            }

            // 9. Map to response
            var response = MapToResponse(appointmentWithDetails);

            return BaseResponse<AppointmentResponse>.SuccessResponse(
                response,
                "Cập nhật lịch hẹn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật Appointment, AppointmentId: {AppointmentId}, StaffId: {StaffId}", appointmentId, staffId);
            return BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật Appointment");
        }
    }

    /// <summary>
    /// UC42: Hủy Appointment
    /// </summary>
    public async Task<BaseResponse<AppointmentResponse>> CancelAppointmentAsync(
        Guid appointmentId,
        Guid staffId)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get appointment
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy lịch hẹn");
            }

            // 3. Validate appointment belongs to this Staff
            if (appointment.StaffId != staffId)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Bạn không có quyền hủy lịch hẹn này");
            }

            // 4. Validate appointment is not already canceled or completed
            if (appointment.Status == "CANCELED")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Lịch hẹn đã được hủy trước đó");
            }

            if (appointment.Status == "COMPLETED")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể hủy lịch hẹn đã hoàn thành");
            }

            // 5. Cancel appointment
            appointment.Status = "CANCELED";
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã hủy Appointment {AppointmentId}",
                staffId,
                appointmentId);

            // 6. Get appointment with details for response
            var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointmentWithDetails == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy Appointment vừa hủy");
            }

            // 7. Send notifications to Buyer and Seller
            var buyerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = appointmentWithDetails.BuyerId,
                NotificationType = "APPOINTMENT_CANCELED",
                Title = "Lịch hẹn đã bị hủy",
                Content = $"Lịch hẹn xem sản phẩm đã bị hủy. Vui lòng liên hệ Staff để đặt lịch mới.",
                RelatedId = appointmentId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = appointmentWithDetails.SellerId,
                NotificationType = "APPOINTMENT_CANCELED",
                Title = "Lịch hẹn đã bị hủy",
                Content = $"Lịch hẹn xem sản phẩm đã bị hủy. Vui lòng liên hệ Staff để đặt lịch mới.",
                RelatedId = appointmentId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(buyerNotification);
            await _notificationRepository.CreateAsync(sellerNotification);
            await _notificationRepository.SaveChangesAsync();

            // 8. Map to response
            var response = MapToResponse(appointmentWithDetails);

            return BaseResponse<AppointmentResponse>.SuccessResponse(
                response,
                "Hủy lịch hẹn thành công. Thông báo đã được gửi cho Buyer và Seller.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hủy Appointment, AppointmentId: {AppointmentId}, StaffId: {StaffId}", appointmentId, staffId);
            return BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi hủy Appointment");
        }
    }

    /// <summary>
    /// UC42: Cập nhật trạng thái Appointment
    /// </summary>
    public async Task<BaseResponse<AppointmentResponse>> UpdateAppointmentStatusAsync(
        Guid appointmentId,
        Guid staffId,
        AppointmentStatusUpdateRequest request)
    {
        try
        {
            // 1. Validate Staff exists
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.Role != "STAFF")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Staff không tồn tại hoặc không có quyền");
            }

            // 2. Get appointment
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy lịch hẹn");
            }

            // 3. Validate appointment belongs to this Staff
            if (appointment.StaffId != staffId)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Bạn không có quyền cập nhật trạng thái lịch hẹn này");
            }

            // 4. Validate status transition
            var validStatuses = new[] { "CONFIRMED", "CANCELED", "COMPLETED" };
            var newStatus = request.Status.ToUpper();
            if (!validStatuses.Contains(newStatus))
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Trạng thái không hợp lệ");
            }

            // 5. Validate status transition rules
            if (appointment.Status == "CANCELED" && newStatus != "CANCELED")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể thay đổi trạng thái từ CANCELED sang trạng thái khác");
            }

            if (appointment.Status == "COMPLETED" && newStatus != "COMPLETED")
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể thay đổi trạng thái từ COMPLETED sang trạng thái khác");
            }

            // 6. Save old status for logging
            var oldStatus = appointment.Status;

            // 7. Update status
            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            // 8. Update notes if provided
            if (!string.IsNullOrEmpty(request.Notes))
            {
                var currentNotes = appointment.Notes;
                if (string.IsNullOrEmpty(currentNotes))
                {
                    appointment.Notes = request.Notes;
                }
                else
                {
                    appointment.Notes = $"{currentNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {request.Notes}";
                }
            }

            // 9. Save changes
            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Staff {StaffId} đã cập nhật trạng thái Appointment {AppointmentId} từ {OldStatus} sang {NewStatus}",
                staffId,
                appointmentId,
                oldStatus,
                newStatus);

            // 10. Get appointment with details for response
            var appointmentWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointmentWithDetails == null)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không tìm thấy Appointment vừa cập nhật");
            }

            // 11. Send notifications to Buyer and Seller
            var statusText = newStatus switch
            {
                "CONFIRMED" => "đã xác nhận",
                "CANCELED" => "đã bị hủy",
                "COMPLETED" => "đã hoàn thành",
                _ => "đã được cập nhật"
            };

            var buyerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = appointmentWithDetails.BuyerId,
                NotificationType = "APPOINTMENT_STATUS_UPDATED",
                Title = $"Lịch hẹn đã {statusText}",
                Content = $"Lịch hẹn xem sản phẩm \"{appointmentWithDetails.Post?.Title ?? "N/A"}\" đã {statusText}.{(!string.IsNullOrEmpty(request.Notes) ? $" Ghi chú: {request.Notes}" : "")}",
                RelatedId = appointmentId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var sellerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = appointmentWithDetails.SellerId,
                NotificationType = "APPOINTMENT_STATUS_UPDATED",
                Title = $"Lịch hẹn đã {statusText}",
                Content = $"Lịch hẹn xem sản phẩm \"{appointmentWithDetails.Post?.Title ?? "N/A"}\" đã {statusText}.{(!string.IsNullOrEmpty(request.Notes) ? $" Ghi chú: {request.Notes}" : "")}",
                RelatedId = appointmentId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(buyerNotification);
            await _notificationRepository.CreateAsync(sellerNotification);
            await _notificationRepository.SaveChangesAsync();

            // 12. Map to response
            var response = MapToResponse(appointmentWithDetails);

            return BaseResponse<AppointmentResponse>.SuccessResponse(
                response,
                $"Cập nhật trạng thái lịch hẹn thành công. Trạng thái mới: {newStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Appointment, AppointmentId: {AppointmentId}, StaffId: {StaffId}", appointmentId, staffId);
            return BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật trạng thái Appointment");
        }
    }

    private AppointmentResponse MapToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            AppointmentId = appointment.Id,
            LeadId = appointment.LeadId,
            PostId = appointment.PostId,
            PostTitle = appointment.Post?.Title ?? string.Empty,
            BuyerId = appointment.BuyerId,
            BuyerName = appointment.Buyer?.FullName ?? appointment.Buyer?.Email ?? string.Empty,
            BuyerEmail = appointment.Buyer?.Email ?? string.Empty,
            SellerId = appointment.SellerId,
            SellerName = appointment.Seller?.FullName ?? appointment.Seller?.Email ?? string.Empty,
            SellerEmail = appointment.Seller?.Email ?? string.Empty,
            StaffId = appointment.StaffId,
            StaffName = appointment.Staff?.FullName ?? appointment.Staff?.Email ?? string.Empty,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Location = appointment.Location,
            Notes = appointment.Notes,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}

