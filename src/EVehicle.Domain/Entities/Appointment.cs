using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid AppointmentId => Id;
    public Guid LeadId { get; set; }
    public Guid PostId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "CONFIRMED"; // CONFIRMED, CANCELED, COMPLETED

    // Navigation properties
    public Lead Lead { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User Staff { get; set; } = null!;
}

