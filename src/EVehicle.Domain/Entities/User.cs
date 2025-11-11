using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? IdNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string? SocialLoginProvider { get; set; }
    public string? SocialLoginId { get; set; }
    public string Role { get; set; } = "MEMBER"; // MEMBER, STAFF, ADMIN
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, BANNED, SUSPENDED, PENDING_VERIFICATION
    public bool EmailVerified { get; set; } = false;
    public DateTime? EmailVerifiedAt { get; set; }

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<PostStaffAssignment> PostStaffAssignments { get; set; } = new List<PostStaffAssignment>();
    public ICollection<UserPackageCredits> UserPackageCredits { get; set; } = new List<UserPackageCredits>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<ProductComparison> ProductComparisons { get; set; } = new List<ProductComparison>();
    public ICollection<Lead> LeadsAsBuyer { get; set; } = new List<Lead>();
    public ICollection<Lead> LeadsAsStaff { get; set; } = new List<Lead>();
    public ICollection<Appointment> AppointmentsAsBuyer { get; set; } = new List<Appointment>();
    public ICollection<Appointment> AppointmentsAsSeller { get; set; } = new List<Appointment>();
    public ICollection<Appointment> AppointmentsAsStaff { get; set; } = new List<Appointment>();
    public ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
    public ICollection<Contract> ContractsCreated { get; set; } = new List<Contract>();
    public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
    public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
    public ICollection<RatingReply> RatingReplies { get; set; } = new List<RatingReply>();
    public ICollection<ChatRoom> ChatRoomsAsBuyer { get; set; } = new List<ChatRoom>();
    public ICollection<ChatRoom> ChatRoomsAsSeller { get; set; } = new List<ChatRoom>();
    public ICollection<ChatRoom> ChatRoomsAsStaff { get; set; } = new List<ChatRoom>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

