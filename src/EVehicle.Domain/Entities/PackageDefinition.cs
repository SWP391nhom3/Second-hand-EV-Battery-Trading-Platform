namespace EVehicle.Domain.Entities;

public class PackageDefinition
{
    public int PackageId { get; set; }
    public string Name { get; set; } = string.Empty; // Basic, Premium, Luxury
    public decimal Price { get; set; }
    public int CreditsCount { get; set; } // Số lượng credits được cấp khi mua gói
    public int PriorityLevel { get; set; } // 3=Luxury, 2=Premium, 1=Basic
    public int MaxImages { get; set; } = 5; // Số lượng ảnh tối đa cho phép
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserPackageCredits> UserPackageCredits { get; set; } = new List<UserPackageCredits>();
    public ICollection<PostSubscription> PostSubscriptions { get; set; } = new List<PostSubscription>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

