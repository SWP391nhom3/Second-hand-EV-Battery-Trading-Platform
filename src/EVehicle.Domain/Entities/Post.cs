using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Post : BaseEntity
{
    public Guid UserId { get; set; } // Người bán
    public int CategoryId { get; set; }

    // Thông tin cơ bản
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public string? Location { get; set; }

    // Thông tin chung cho cả Xe và Pin
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal BatteryCapacityCurrent { get; set; }
    public int? ChargeCount { get; set; }
    public int ProductionYear { get; set; }
    public string Condition { get; set; } = string.Empty;

    // Trường riêng cho Xe điện (NULL nếu là Pin)
    public int? Mileage { get; set; }

    // Đấu giá
    public bool AuctionEnabled { get; set; } = false;
    public decimal? StartingBid { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime? AuctionEndTime { get; set; }

    // Quản lý trạng thái
    public string Status { get; set; } = "PENDING"; // DRAFT, PENDING, APPROVED, DENIED
    public bool IsActive { get; set; } = true;
    public bool IsSold { get; set; } = false;
    public string? RejectionReason { get; set; }

    // Package được chọn khi tạo bài đăng (để Admin biết package nào đã chọn khi duyệt)
    public int? SelectedPackageId { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; } // Admin đã duyệt
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; } // Admin đã từ chối
    public DateTime BumpedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
    public ICollection<PostStaffAssignment> PostStaffAssignments { get; set; } = new List<PostStaffAssignment>();
    public ICollection<PostSubscription> PostSubscriptions { get; set; } = new List<PostSubscription>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<ProductComparison> ProductComparisons { get; set; } = new List<ProductComparison>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
    public ICollection<AIPriceSuggestion> AIPriceSuggestions { get; set; } = new List<AIPriceSuggestion>();
}

