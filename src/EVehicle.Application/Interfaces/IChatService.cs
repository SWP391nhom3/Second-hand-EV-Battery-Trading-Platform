using EVehicle.Application.DTOs.Chat;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Chat Service
/// </summary>
public interface IChatService
{
    /// <summary>
    /// UC35: Gửi tin nhắn
    /// </summary>
    Task<BaseResponse<MessageResponse>> SendMessageAsync(Guid userId, MessageCreateRequest request);

    /// <summary>
    /// UC36: Lấy lịch sử chat
    /// </summary>
    Task<PagedResponse<MessageResponse>> GetChatHistoryAsync(Guid userId, ChatHistoryRequest request);

    /// <summary>
    /// Lấy danh sách phòng chat của user
    /// </summary>
    Task<PagedResponse<ChatRoomResponse>> GetChatRoomsAsync(Guid userId, ChatRoomsListRequest request);

    /// <summary>
    /// Lấy thông tin phòng chat
    /// </summary>
    Task<BaseResponse<ChatRoomResponse>> GetChatRoomAsync(Guid userId, Guid roomId);

    /// <summary>
    /// Tạo phòng chat cho Lead (Staff)
    /// </summary>
    Task<BaseResponse<ChatRoomResponse>> CreateRoomForLeadAsync(Guid staffId, ChatRoomCreateRequest request);
}

