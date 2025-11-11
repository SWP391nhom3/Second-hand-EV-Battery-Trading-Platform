namespace EVehicle.Application.DTOs.Chat;

/// <summary>
/// Request tạo phòng chat từ Lead
/// </summary>
public class ChatRoomCreateRequest
{
    public Guid LeadId { get; set; }
}


