using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EVehicleManagementAPI.DBconnect
{
    public class EVehicleDbContext : DbContext
    {
        public EVehicleDbContext(DbContextOptions<EVehicleDbContext> options)
            : base(options)
        {
        }

        // -------------------- DbSet declarations --------------------
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<Battery> Batteries { get; set; }
        public DbSet<BatteryModel> BatteryModels { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostPackage> PostPackages { get; set; }
        public DbSet<PostPackageSub> PostPackageSubs { get; set; }
        public DbSet<PostRequest> PostRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ServiceFee> ServiceFees { get; set; }
        public DbSet<ConstructFee> ConstructFees { get; set; }
        public DbSet<Construct> Constructs { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }

        // -------------------- OnModelCreating --------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1️⃣ Ngăn lỗi Multiple Cascade Paths toàn cục ---
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // --- 2️⃣ Khai báo khóa chính ---
            modelBuilder.Entity<PostPackage>().HasKey(pp => pp.PackageId);
            modelBuilder.Entity<PostPackageSub>().HasKey(pps => pps.Id);
            modelBuilder.Entity<ServiceFee>().HasKey(sf => sf.Id);
            modelBuilder.Entity<ConstructFee>().HasKey(cf => cf.Id);
            modelBuilder.Entity<Construct>().HasKey(c => c.ConstructId);
            modelBuilder.Entity<PostRequest>().HasKey(pr => pr.Id);
            modelBuilder.Entity<ExternalLogin>().HasKey(el => el.Id);
            modelBuilder.Entity<OtpCode>().HasKey(o => o.Id);

            // --- 3️⃣ Cấu hình quan hệ ---

            // Account → Role (N:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account → Member (1:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Member)
                .WithOne(m => m.Account)
                .HasForeignKey<Member>(m => m.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member → Vehicle (1:N)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Member)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // VehicleModel → Vehicle (1:N)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleModel)
                .WithMany(vm => vm.Vehicles)
                .HasForeignKey(v => v.VehicleModelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Member → Battery (1:N)
            modelBuilder.Entity<Battery>()
                .HasOne(b => b.Member)
                .WithMany(m => m.Batteries)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // BatteryModel → Battery (1:N)
            modelBuilder.Entity<Battery>()
                .HasOne(b => b.BatteryModel)
                .WithMany(bm => bm.Batteries)
                .HasForeignKey(b => b.BatteryModelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Member → Post (1:N)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Member)
                .WithMany(m => m.Posts)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post → Staff (Member) (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Staff)
                .WithMany()
                .HasForeignKey(p => p.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Post → Vehicle (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.Posts)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Post → Battery (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Battery)
                .WithMany(b => b.Posts)
                .HasForeignKey(p => p.BatteryId)
                .OnDelete(DeleteBehavior.NoAction);

            // PostPackage → PostPackageSub (1:N)
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.PostPackage)
                .WithMany(pp => pp.PostPackageSubs)
                .HasForeignKey(pps => pps.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post → PostPackageSub (1:N)
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Post)
                .WithMany(p => p.PostPackageSubs)
                .HasForeignKey(pps => pps.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member → PostPackageSub (1:N)
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Member)
                .WithMany(m => m.PostPackageSubs)
                .HasForeignKey(pps => pps.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment → PostPackageSub (1:N)
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Payment)
                .WithMany(p => p.PostPackageSubs)
                .HasForeignKey(pps => pps.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ConstructFee ↔ ServiceFee (1:1)
            modelBuilder.Entity<ServiceFee>()
                .HasOne(sf => sf.ConstructFee)
                .WithOne(cf => cf.ServiceFee)
                .HasForeignKey<ServiceFee>(sf => sf.ConstructFeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Construct → ConstructFee (1:N)
            modelBuilder.Entity<ConstructFee>()
                .HasOne(cf => cf.Construct)
                .WithMany(c => c.ConstructFees)
                .HasForeignKey(cf => cf.ConstructId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member → ConstructFee (1:N)
            modelBuilder.Entity<ConstructFee>()
                .HasOne(cf => cf.Member)
                .WithMany(m => m.ConstructFees)
                .HasForeignKey(cf => cf.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment → Construct (1:N)
            modelBuilder.Entity<Construct>()
                .HasOne(c => c.Payment)
                .WithMany(p => p.Constructs)
                .HasForeignKey(c => c.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment ↔ Member (Buyer/Seller)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Buyer)
                .WithMany(m => m.PaymentsAsBuyer)
                .HasForeignKey(p => p.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Seller)
                .WithMany(m => m.PaymentsAsSeller)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            // PostRequest relations
            modelBuilder.Entity<PostRequest>()
                .HasOne(pr => pr.Post)
                .WithMany(p => p.PostRequests)
                .HasForeignKey(pr => pr.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostRequest>()
                .HasOne(pr => pr.Buyer)
                .WithMany(m => m.PostRequests)
                .HasForeignKey(pr => pr.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostRequest>()
                .HasOne(pr => pr.Construct)
                .WithMany(c => c.PostRequests)
                .HasForeignKey(pr => pr.ConstructId)
                .OnDelete(DeleteBehavior.SetNull);

            // Account ↔ ExternalLogin (1:N)
            modelBuilder.Entity<ExternalLogin>()
                .HasOne(el => el.Account)
                .WithMany(a => a.ExternalLogins)
                .HasForeignKey(el => el.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExternalLogin>()
                .HasIndex(el => new { el.Provider, el.ProviderKey })
                .IsUnique();

            // OTP Codes (1:N)
            modelBuilder.Entity<OtpCode>()
                .HasOne(o => o.Account)
                .WithMany(a => a.OtpCodes)
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OtpCode>()
                .HasIndex(o => new { o.Email, o.Purpose });
        }
    }
}
