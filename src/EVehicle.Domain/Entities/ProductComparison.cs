using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class ProductComparison : BaseEntity
{
    public Guid ComparisonId => Id;
    public Guid? UserId { get; set; } // NULL nếu session-based
    public string? SessionId { get; set; } // Nếu không đăng nhập
    public Guid PostId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Post Post { get; set; } = null!;
}

