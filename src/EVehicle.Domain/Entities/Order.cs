using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Order : BaseEntity
{
    public Guid OrderId => Id;
    public Guid? LeadId { get; set; } // Liên kết với Lead (nếu có)
    public Guid PostId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; } // Lấy từ Post.UserId
    public Guid? StaffId { get; set; } // Staff hỗ trợ giao dịch
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = "PENDING_PAYMENT"; 
    // PENDING_PAYMENT, PAID, CONFIRMED, SHIPPING, DELIVERED, COMPLETED, CANCELLED
    public string? PaymentMethod { get; set; }
    public string? ShippingAddress { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Lead? Lead { get; set; }
    public Post Post { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User? Staff { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}

