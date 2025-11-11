using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Lead : BaseEntity
{
    public Guid LeadId => Id;
    public Guid PostId { get; set; }
    public Guid BuyerId { get; set; } // Người mua
    public Guid? StaffId { get; set; } // Staff được gán (NULL nếu chưa gán)
    public Guid? AssignedBy { get; set; } // Admin gán Staff
    public string LeadType { get; set; } = "SCHEDULE_VIEW"; // SCHEDULE_VIEW, AUCTION_WINNER
    public string Status { get; set; } = "NEW"; // NEW, ASSIGNED, CONTACTED, SCHEDULED, SUCCESSFUL, FAILED
    public decimal? FinalPrice { get; set; } // Giá cuối cùng (dùng cho đấu giá)
    public DateTime? AssignedAt { get; set; } // Thời gian được gán Staff
    public DateTime? ClosedAt { get; set; } // Thời gian đóng Lead (thành công hoặc thất bại)
    public string? Notes { get; set; } // Ghi chú của Staff

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User? Staff { get; set; }
    public User? AssignedByUser { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}

