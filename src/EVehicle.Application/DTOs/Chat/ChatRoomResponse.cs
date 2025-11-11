namespace EVehicle.Application.DTOs.Chat;

/// <summary>
/// Response DTO cho phòng chat
/// </summary>
public class ChatRoomResponse
{
    /// <summary>
    /// ID phòng chat
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string PostTitle { get; set; } = string.Empty;

    /// <summary>
    /// Ảnh đại diện bài đăng
    /// </summary>
    public string? PostThumbnail { get; set; }

    /// <summary>
    /// ID người mua
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Tên người mua
    /// </summary>
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar người mua
    /// </summary>
    public string? BuyerAvatar { get; set; }

    /// <summary>
    /// ID người bán
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar người bán
    /// </summary>
    public string? SellerAvatar { get; set; }

    /// <summary>
    /// ID Staff (nếu có)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Tên Staff (nếu có)
    /// </summary>
    public string? StaffName { get; set; }

    /// <summary>
    /// Avatar Staff (nếu có)
    /// </summary>
    public string? StaffAvatar { get; set; }

    /// <summary>
    /// Tin nhắn cuối cùng
    /// </summary>
    public MessageResponse? LastMessage { get; set; }

    /// <summary>
    /// Số tin nhắn chưa đọc
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Thời gian tin nhắn cuối cùng
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Thời gian tạo phòng chat
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

