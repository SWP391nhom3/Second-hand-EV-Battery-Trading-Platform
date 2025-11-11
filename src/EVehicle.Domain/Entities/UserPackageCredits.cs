using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class UserPackageCredits : BaseEntity
{
    public Guid UserCreditId => Id;
    public Guid UserId { get; set; }
    public int PackageId { get; set; }
    public int CreditsRemaining { get; set; } = 0; // Số credits còn lại
    public int TotalCredits { get; set; } // Tổng số credits đã mua
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // Thời gian hết hạn (nếu có)

    // Navigation properties
    public User User { get; set; } = null!;
    public PackageDefinition Package { get; set; } = null!;
    public ICollection<PostSubscription> PostSubscriptions { get; set; } = new List<PostSubscription>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

