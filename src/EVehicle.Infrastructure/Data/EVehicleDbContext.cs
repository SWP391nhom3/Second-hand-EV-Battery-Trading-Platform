using Microsoft.EntityFrameworkCore;
using EVehicle.Domain.Entities;

namespace EVehicle.Infrastructure.Data;

public class EVehicleDbContext : DbContext
{
    public EVehicleDbContext(DbContextOptions<EVehicleDbContext> options) : base(options)
    {
    }

    // User & Access Control
    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerificationOtp> EmailVerificationOtps { get; set; }
    
    // Products & Posts
    public DbSet<Category> Categories { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }
    public DbSet<PostStaffAssignment> PostStaffAssignments { get; set; }
    
    // Packages & Credits
    public DbSet<PackageDefinition> PackageDefinitions { get; set; }
    public DbSet<UserPackageCredits> UserPackageCredits { get; set; }
    public DbSet<PostSubscription> PostSubscriptions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    // Search & Purchase
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<ProductComparison> ProductComparisons { get; set; }
    
    // Brokerage Workflow
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    
    // Transactions & Contracts
    public DbSet<Order> Orders { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractTemplate> ContractTemplates { get; set; }
    
    // Ratings & Trust
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<RatingReply> RatingReplies { get; set; }
    
    // Communication
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    // AI & Analytics
    public DbSet<AIPriceSuggestion> AIPriceSuggestions { get; set; }
    public DbSet<MarketData> MarketData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigurePostImage(modelBuilder);
        ConfigurePostStaffAssignment(modelBuilder);
        ConfigurePackageDefinition(modelBuilder);
        ConfigureUserPackageCredits(modelBuilder);
        ConfigurePostSubscription(modelBuilder);
        ConfigurePayment(modelBuilder);
        ConfigureFavorite(modelBuilder);
        ConfigureBid(modelBuilder);
        ConfigureProductComparison(modelBuilder);
        ConfigureLead(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureContract(modelBuilder);
        ConfigureContractTemplate(modelBuilder);
        ConfigureRating(modelBuilder);
        ConfigureRatingReply(modelBuilder);
        ConfigureChatRoom(modelBuilder);
        ConfigureChatMessage(modelBuilder);
        ConfigureNotification(modelBuilder);
        ConfigureAIPriceSuggestion(modelBuilder);
        ConfigureMarketData(modelBuilder);
        ConfigureEmailVerificationOtp(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.IdNumber).HasColumnName("id_number").HasMaxLength(20);
            entity.Property(e => e.IdFrontImageUrl).HasColumnName("id_front_image_url");
            entity.Property(e => e.IdBackImageUrl).HasColumnName("id_back_image_url");
            entity.Property(e => e.SocialLoginProvider).HasColumnName("social_login_provider").HasMaxLength(50);
            entity.Property(e => e.SocialLoginId).HasColumnName("social_login_id").HasMaxLength(255);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(20).IsRequired().HasDefaultValue("MEMBER");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified").HasDefaultValue(false);
            entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("category_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private void ConfigurePost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.SuggestedPrice).HasColumnName("suggested_price").HasPrecision(15, 2);
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
            entity.Property(e => e.BatteryCapacityCurrent).HasColumnName("battery_capacity_current").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.ChargeCount).HasColumnName("charge_count");
            entity.Property(e => e.ProductionYear).HasColumnName("production_year").IsRequired();
            entity.Property(e => e.Condition).HasColumnName("condition").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Mileage).HasColumnName("mileage");
            entity.Property(e => e.AuctionEnabled).HasColumnName("auction_enabled").HasDefaultValue(false);
            entity.Property(e => e.StartingBid).HasColumnName("starting_bid").HasPrecision(15, 2);
            entity.Property(e => e.BuyNowPrice).HasColumnName("buy_now_price").HasPrecision(15, 2);
            entity.Property(e => e.AuctionEndTime).HasColumnName("auction_end_time");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsSold).HasColumnName("is_sold").HasDefaultValue(false);
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.SelectedPackageId).HasColumnName("selected_package_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.RejectedAt).HasColumnName("rejected_at");
            entity.Property(e => e.RejectedBy).HasColumnName("rejected_by");
            entity.Property(e => e.BumpedAt).HasColumnName("bumped_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsSold);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.Model);
            entity.HasIndex(e => e.ProductionYear);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.BumpedAt);
            entity.HasIndex(e => new { e.Status, e.IsActive, e.IsSold });
            entity.HasIndex(e => new { e.CategoryId, e.Brand, e.Model });
        });
    }

    private void ConfigurePostImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.ToTable("Post_Images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("image_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired();
            entity.Property(e => e.IsThumbnail).HasColumnName("is_thumbnail").HasDefaultValue(false);
            entity.Property(e => e.IsProof).HasColumnName("is_proof").HasDefaultValue(false);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.PostImages)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PostId);
        });
    }

    private void ConfigurePostStaffAssignment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostStaffAssignment>(entity =>
        {
            entity.ToTable("Post_Staff_Assignments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("assignment_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.StaffId).HasColumnName("staff_id").IsRequired();
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by").IsRequired();
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.PostStaffAssignments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(u => u.PostStaffAssignments)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedByUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PostId).IsUnique();
            entity.HasIndex(e => e.StaffId);
        });
    }

    private void ConfigurePackageDefinition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PackageDefinition>(entity =>
        {
            entity.ToTable("Package_Definitions");
            entity.HasKey(e => e.PackageId);
            entity.Property(e => e.PackageId).HasColumnName("package_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.CreditsCount).HasColumnName("credits_count").IsRequired();
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level").IsRequired();
            entity.Property(e => e.MaxImages).HasColumnName("max_images").HasDefaultValue(5);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private void ConfigureUserPackageCredits(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPackageCredits>(entity =>
        {
            entity.ToTable("User_Package_Credits");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("user_credit_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.PackageId).HasColumnName("package_id").IsRequired();
            entity.Property(e => e.CreditsRemaining).HasColumnName("credits_remaining").HasDefaultValue(0);
            entity.Property(e => e.TotalCredits).HasColumnName("total_credits").IsRequired();
            entity.Property(e => e.PurchasedAt).HasColumnName("purchased_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPackageCredits)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Package)
                .WithMany(p => p.UserPackageCredits)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.PackageId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PackageId);
        });
    }

    private void ConfigurePostSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostSubscription>(entity =>
        {
            entity.ToTable("Post_Subscriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("subscription_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.UserCreditId).HasColumnName("user_credit_id").IsRequired();
            entity.Property(e => e.PackageId).HasColumnName("package_id").IsRequired();
            entity.Property(e => e.CreditsUsed).HasColumnName("credits_used").HasDefaultValue(1);
            entity.Property(e => e.AppliedAt).HasColumnName("applied_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.PostSubscriptions)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UserPackageCredits)
                .WithMany(c => c.PostSubscriptions)
                .HasForeignKey(e => e.UserCreditId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Package)
                .WithMany(p => p.PostSubscriptions)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PostId).IsUnique();
            entity.HasIndex(e => e.UserCreditId);
            entity.HasIndex(e => e.PackageId);
        });
    }

    private void ConfigurePayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("payment_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.UserCreditId).HasColumnName("user_credit_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.PaymentGateway).HasColumnName("payment_gateway").HasMaxLength(20).IsRequired();
            entity.Property(e => e.TransactionCode).HasColumnName("transaction_code").HasMaxLength(255);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.PaymentType).HasColumnName("payment_type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UserPackageCredits)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.UserCreditId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Package)
                .WithMany(p => p.Payments)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PaymentType);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureFavorite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("Favorites");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("favorite_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Favorites)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
        });
    }

    private void ConfigureBid(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.ToTable("Bids");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("bid_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.BidAmount).HasColumnName("bid_amount").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.IsWinningBid).HasColumnName("is_winning_bid").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Bids)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureProductComparison(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductComparison>(entity =>
        {
            entity.ToTable("Product_Comparisons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("comparison_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id").HasMaxLength(255);
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProductComparisons)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.ProductComparisons)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureLead(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("lead_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.LeadType).HasColumnName("lead_type").HasMaxLength(30).IsRequired().HasDefaultValue("SCHEDULE_VIEW");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30).IsRequired().HasDefaultValue("NEW");
            entity.Property(e => e.FinalPrice).HasColumnName("final_price").HasPrecision(15, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Leads)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Buyer)
                .WithMany(u => u.LeadsAsBuyer)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(u => u.LeadsAsStaff)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedByUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LeadType);
        });
    }

    private void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("appointment_id");
            entity.Property(e => e.LeadId).HasColumnName("lead_id").IsRequired();
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
            entity.Property(e => e.SellerId).HasColumnName("seller_id").IsRequired();
            entity.Property(e => e.StaffId).HasColumnName("staff_id").IsRequired();
            entity.Property(e => e.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Location).HasColumnName("location").IsRequired();
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("CONFIRMED");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Appointments)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Buyer)
                .WithMany(u => u.AppointmentsAsBuyer)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Seller)
                .WithMany(u => u.AppointmentsAsSeller)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(u => u.AppointmentsAsStaff)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("order_id");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
            entity.Property(e => e.SellerId).HasColumnName("seller_id").IsRequired();
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.FinalPrice).HasColumnName("final_price").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("PENDING_PAYMENT");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Orders)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Orders)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Buyer)
                .WithMany(u => u.OrdersAsBuyer)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Seller)
                .WithMany(u => u.OrdersAsSeller)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany()
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
            // Note: Staff navigation property trong Order không có trong User entity để tránh circular reference

            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureContract(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("contract_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.ContractTemplateId).HasColumnName("contract_template_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ContractContent).HasColumnName("contract_content");
            entity.Property(e => e.BuyerSignature).HasColumnName("buyer_signature");
            entity.Property(e => e.SellerSignature).HasColumnName("seller_signature");
            entity.Property(e => e.BuyerSignedAt).HasColumnName("buyer_signed_at");
            entity.Property(e => e.SellerSignedAt).HasColumnName("seller_signed_at");
            entity.Property(e => e.ContractPdfUrl).HasColumnName("contract_pdf_url");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("DRAFT");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.SignedAt).HasColumnName("signed_at");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Contracts)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Contracts)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ContractTemplate)
                .WithMany(t => t.Contracts)
                .HasForeignKey(e => e.ContractTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.ContractsCreated)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.CreatedBy);
        });
    }

    private void ConfigureContractTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractTemplate>(entity =>
        {
            entity.ToTable("Contract_Templates");
            entity.HasKey(e => e.TemplateId);
            entity.Property(e => e.TemplateId).HasColumnName("template_id").ValueGeneratedOnAdd();
            entity.Property(e => e.TemplateName).HasColumnName("template_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.TemplateContent).HasColumnName("template_content").IsRequired();
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.ContractTemplates)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureRating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("Ratings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("rating_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.RaterId).HasColumnName("rater_id").IsRequired();
            entity.Property(e => e.RateeId).HasColumnName("ratee_id").IsRequired();
            entity.Property(e => e.RateeRole).HasColumnName("ratee_role").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Score).HasColumnName("score").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Ratings)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Rater)
                .WithMany(u => u.RatingsGiven)
                .HasForeignKey(e => e.RaterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Ratee)
                .WithMany(u => u.RatingsReceived)
                .HasForeignKey(e => e.RateeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.OrderId, e.RaterId, e.RateeId }).IsUnique();
        });
    }

    private void ConfigureRatingReply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RatingReply>(entity =>
        {
            entity.ToTable("Rating_Replies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("reply_id");
            entity.Property(e => e.RatingId).HasColumnName("rating_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ReplyContent).HasColumnName("reply_content").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Rating)
                .WithMany(r => r.RatingReplies)
                .HasForeignKey(e => e.RatingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RatingReplies)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureChatRoom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.ToTable("Chat_Rooms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("room_id");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
            entity.Property(e => e.SellerId).HasColumnName("seller_id").IsRequired();
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastMessageAt).HasColumnName("last_message_at");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.ChatRooms)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.ChatRooms)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Buyer)
                .WithMany(u => u.ChatRoomsAsBuyer)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Seller)
                .WithMany(u => u.ChatRoomsAsSeller)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(u => u.ChatRoomsAsStaff)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.StaffId);
        });
    }

    private void ConfigureChatMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("Chat_Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("message_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(20).HasDefaultValue("TEXT");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.ChatRoom)
                .WithMany(r => r.ChatMessages)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.RoomId);
        });
    }

    private void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("notification_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.NotificationType).HasColumnName("notification_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.RelatedId).HasColumnName("related_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureAIPriceSuggestion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AIPriceSuggestion>(entity =>
        {
            entity.ToTable("AI_Price_Suggestions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("suggestion_id");
            entity.Property(e => e.PostId).HasColumnName("post_id").IsRequired();
            entity.Property(e => e.SuggestedPrice).HasColumnName("suggested_price").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.ConfidenceScore).HasColumnName("confidence_score").HasPrecision(5, 2);
            entity.Property(e => e.Factors).HasColumnName("factors");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.AIPriceSuggestions)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureMarketData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MarketData>(entity =>
        {
            entity.ToTable("Market_Data");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("data_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(100);
            entity.Property(e => e.Model).HasColumnName("model").HasMaxLength(100);
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.SohPercentage).HasColumnName("soh_percentage").HasPrecision(5, 2);
            entity.Property(e => e.Mileage).HasColumnName("mileage");
            entity.Property(e => e.SellingPrice).HasColumnName("selling_price").HasPrecision(15, 2).IsRequired();
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.MarketData)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureEmailVerificationOtp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailVerificationOtp>(entity =>
        {
            entity.ToTable("Email_Verification_Otps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("otp_id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.OtpCode).HasColumnName("otp_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
            entity.Property(e => e.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.Email, e.IsUsed, e.ExpiresAt });
        });
    }
}
