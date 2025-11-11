using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class PostSubscription : BaseEntity
{
    public Guid SubscriptionId => Id;
    public Guid PostId { get; set; }
    public Guid UserCreditId { get; set; } // Liên kết với credits đã sử dụng
    public int PackageId { get; set; }
    public int CreditsUsed { get; set; } = 1; // Số credits đã sử dụng (thường là 1)
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow; // Thời gian áp dụng gói (khi admin duyệt)

    // Navigation properties
    public Post Post { get; set; } = null!;
    public UserPackageCredits UserPackageCredits { get; set; } = null!;
    public PackageDefinition Package { get; set; } = null!;
}

