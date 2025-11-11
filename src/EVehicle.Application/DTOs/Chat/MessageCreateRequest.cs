using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Chat;

/// <summary>
/// Request DTO cho việc gửi tin nhắn (UC35)
/// </summary>
public class MessageCreateRequest
{
    /// <summary>
    /// ID phòng chat (nếu đã có phòng chat)
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// ID bài đăng (bắt buộc nếu chưa có RoomId)
    /// </summary>
    public Guid? PostId { get; set; }

    /// <summary>
    /// Nội dung tin nhắn (bắt buộc cho TEXT, không cần cho IMAGE/FILE)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Loại tin nhắn (TEXT, IMAGE, FILE)
    /// </summary>
    public string MessageType { get; set; } = "TEXT";

    /// <summary>
    /// File đính kèm (hình ảnh hoặc file) - đã được convert sang FileUploadDto
    /// </summary>
    public FileUploadDto? File { get; set; }
}

