using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class PostStaffAssignment : BaseEntity
{
    public Guid AssignmentId => Id;
    public Guid PostId { get; set; }
    public Guid StaffId { get; set; } // Staff được gán
    public Guid AssignedBy { get; set; } // Admin gán
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User Staff { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}

