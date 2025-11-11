using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Chat Repository
/// </summary>
public interface IChatRepository
{
    /// <summary>
    /// Lấy phòng chat theo ID
    /// </summary>
    Task<ChatRoom?> GetRoomByIdAsync(Guid roomId);

    /// <summary>
    /// Lấy phòng chat theo PostId và BuyerId (tìm phòng chat đã tồn tại)
    /// </summary>
    Task<ChatRoom?> GetRoomByPostAndBuyerAsync(Guid postId, Guid buyerId);

    /// <summary>
    /// Lấy phòng chat theo LeadId
    /// </summary>
    Task<ChatRoom?> GetRoomByLeadIdAsync(Guid leadId);

    /// <summary>
    /// Lấy danh sách phòng chat của user
    /// </summary>
    Task<PagedResult<ChatRoom>> GetRoomsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection);

    /// <summary>
    /// Tạo phòng chat mới
    /// </summary>
    Task<ChatRoom> CreateRoomAsync(ChatRoom room);

    /// <summary>
    /// Cập nhật phòng chat (thêm Staff vào phòng chat)
    /// </summary>
    Task<ChatRoom> UpdateRoomAsync(ChatRoom room);

    /// <summary>
    /// Lấy tin nhắn theo RoomId (có phân trang)
    /// </summary>
    Task<PagedResult<ChatMessage>> GetMessagesByRoomIdAsync(
        Guid roomId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection);

    /// <summary>
    /// Tạo tin nhắn mới
    /// </summary>
    Task<ChatMessage> CreateMessageAsync(ChatMessage message);

    /// <summary>
    /// Đánh dấu tin nhắn đã đọc
    /// </summary>
    Task MarkMessagesAsReadAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Đếm số tin nhắn chưa đọc trong phòng chat
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync();
}

