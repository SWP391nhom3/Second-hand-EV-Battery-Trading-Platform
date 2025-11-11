namespace EVehicle.Application.DTOs.Chat;

/// <summary>
/// Response DTO cho tin nhắn
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// ID tin nhắn
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// ID phòng chat
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// ID người gửi
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tên người gửi
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar người gửi
    /// </summary>
    public string? SenderAvatar { get; set; }

    /// <summary>
    /// Nội dung tin nhắn
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Loại tin nhắn (TEXT, IMAGE, FILE)
    /// </summary>
    public string MessageType { get; set; } = "TEXT";

    /// <summary>
    /// Đã đọc chưa
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

