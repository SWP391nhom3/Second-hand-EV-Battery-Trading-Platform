using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Chat;

/// <summary>
/// Request DTO cho việc lấy lịch sử chat (UC36)
/// </summary>
public class ChatHistoryRequest : PagedRequest
{
    /// <summary>
    /// ID phòng chat
    /// </summary>
    public Guid RoomId { get; set; }
}

