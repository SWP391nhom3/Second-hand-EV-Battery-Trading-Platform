using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Chat;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý Chat (UC35, UC36)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Chats")]
[AuthorizeRoles("MEMBER", "STAFF")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IValidator<MessageCreateRequest> _messageCreateValidator;
    private readonly IValidator<ChatHistoryRequest> _chatHistoryValidator;
    private readonly IValidator<ChatRoomCreateRequest> _chatRoomCreateValidator;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(
        IChatService chatService,
        IValidator<MessageCreateRequest> messageCreateValidator,
        IValidator<ChatHistoryRequest> chatHistoryValidator,
        IValidator<ChatRoomCreateRequest> chatRoomCreateValidator,
        ILogger<ChatsController> logger)
    {
        _chatService = chatService;
        _messageCreateValidator = messageCreateValidator;
        _chatHistoryValidator = chatHistoryValidator;
        _chatRoomCreateValidator = chatRoomCreateValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC35: Gửi tin nhắn
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(BaseResponse<MessageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMessage(
        [FromForm] Guid? roomId,
        [FromForm] Guid? postId,
        [FromForm] string? content,
        [FromForm] string messageType = "TEXT",
        [FromForm] IFormFile? file = null)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<MessageResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Convert IFormFile to FileUploadDto
            var fileDto = await FileHelper.ConvertToFileUploadDtoAsync(file);

            // 3. Create request
            var request = new MessageCreateRequest
            {
                RoomId = roomId,
                PostId = postId,
                Content = content,
                MessageType = messageType,
                File = fileDto
            };

            // 4. Validate request
            var validationResult = await _messageCreateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<MessageResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 5. Call service
            var response = await _chatService.SendMessageAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetChatHistory),
                    new { roomId = response.Data?.RoomId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi tin nhắn");
            return StatusCode(500, BaseResponse<MessageResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gửi tin nhắn"));
        }
    }

    /// <summary>
    /// Tạo phòng chat cho Lead (dành cho Staff)
    /// </summary>
    [HttpPost("rooms")]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<ChatRoomResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRoomForLead([FromBody] ChatRoomCreateRequest request)
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            var validationResult = await _chatRoomCreateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            var response = await _chatService.CreateRoomForLeadAsync(userId.Value, request);

            if (response.Success)
            {
                return StatusCode(StatusCodes.Status201Created, response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo phòng chat từ Lead");
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<ChatRoomResponse>.FailureResponse(
                    ex,
                    "Đã xảy ra lỗi khi tạo phòng chat"));
        }
    }

    /// <summary>
    /// UC36: Xem lịch sử chat
    /// </summary>
    [HttpGet("rooms/{roomId}/messages")]
    [ProducesResponseType(typeof(PagedResponse<MessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChatHistory(
        Guid roomId,
        [FromQuery] ChatHistoryRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<MessageResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Set RoomId from route
            request.RoomId = roomId;

            // 3. Validate request
            var validationResult = await _chatHistoryValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(PagedResponse<MessageResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 4. Call service
            var response = await _chatService.GetChatHistoryAsync(userId.Value, request);

            // 5. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử chat, RoomId: {RoomId}", roomId);
            return StatusCode(500, PagedResponse<MessageResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy lịch sử chat"));
        }
    }

    /// <summary>
    /// Lấy danh sách phòng chat của user
    /// </summary>
    [HttpGet("rooms")]
    [ProducesResponseType(typeof(PagedResponse<ChatRoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChatRooms([FromQuery] ChatRoomsListRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<ChatRoomResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _chatService.GetChatRoomsAsync(userId.Value, request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phòng chat");
            return StatusCode(500, PagedResponse<ChatRoomResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách phòng chat"));
        }
    }

    /// <summary>
    /// Lấy thông tin phòng chat
    /// </summary>
    [HttpGet("rooms/{roomId}")]
    [ProducesResponseType(typeof(BaseResponse<ChatRoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChatRoom(Guid roomId)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Call service
            var response = await _chatService.GetChatRoomAsync(userId.Value, roomId);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin phòng chat, RoomId: {RoomId}", roomId);
            return StatusCode(500, BaseResponse<ChatRoomResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin phòng chat"));
        }
    }
}

