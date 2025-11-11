using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid RoomId => Id;
    public Guid? LeadId { get; set; } // Liên kết với Lead (có Staff được gán)
    public Guid PostId { get; set; } // Liên kết với bài đăng
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? StaffId { get; set; } // Staff được gán (có thể NULL nếu chưa có Lead)
    public DateTime? LastMessageAt { get; set; }

    // Navigation properties
    public Lead? Lead { get; set; }
    public Post Post { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User? Staff { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}

