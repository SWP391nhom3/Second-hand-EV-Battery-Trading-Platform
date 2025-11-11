using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Notifications;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý Thông báo (UC37, UC38)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Notifications")]
[AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IValidator<NotificationMarkReadRequest> _markReadValidator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IValidator<NotificationMarkReadRequest> markReadValidator,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _markReadValidator = markReadValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC37: Xem danh sách thông báo
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNotifications([FromQuery] NotificationSearchRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<NotificationResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _notificationService.GetNotificationsAsync(userId.Value, request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo");
            return StatusCode(500, PagedResponse<NotificationResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách thông báo"));
        }
    }

    /// <summary>
    /// UC38: Đánh dấu thông báo đã đọc
    /// </summary>
    [HttpPut("mark-read")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkNotificationAsRead([FromBody] NotificationMarkReadRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _markReadValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _notificationService.MarkNotificationAsReadAsync(
                userId.Value, 
                request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc");
            return StatusCode(500, BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi đánh dấu thông báo đã đọc"));
        }
    }

    /// <summary>
    /// Lấy số thông báo chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<int>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _notificationService.GetUnreadCountAsync(userId.Value);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy số thông báo chưa đọc");
            return StatusCode(500, BaseResponse<int>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy số thông báo chưa đọc"));
        }
    }
}

