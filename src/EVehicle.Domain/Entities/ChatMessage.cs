using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid MessageId => Id;
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; } // Người gửi
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "TEXT"; // TEXT, IMAGE, FILE
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public ChatRoom ChatRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}

