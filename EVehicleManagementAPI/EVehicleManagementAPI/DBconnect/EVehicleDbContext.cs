using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.DBconnect
{
    public class EVehicleDbContext : DbContext
    {
        public EVehicleDbContext(DbContextOptions<EVehicleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Battery> Batteries { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostPackage> PostPackages { get; set; }
        public DbSet<PostPackageSub> PostPackageSubs { get; set; }
        public DbSet<ServiceFee> ServiceFees { get; set; }
        public DbSet<ConstructFee> ConstructFees { get; set; }
        public DbSet<Construct> Constructs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PostRequest> PostRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<PostPackage>()
                .HasKey(pp => pp.PackageId);

            modelBuilder.Entity<PostPackageSub>()
                .HasKey(pps => pps.Id);

            modelBuilder.Entity<ServiceFee>()
                .HasKey(sf => sf.Id);

            modelBuilder.Entity<ConstructFee>()
                .HasKey(cf => cf.Id);

            modelBuilder.Entity<Construct>()
                .HasKey(c => c.ConstructId);

            modelBuilder.Entity<PostRequest>()
                .HasKey(pr => pr.Id);

            // Configure Account -> Role relationship
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Account -> Member relationship (1:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Member)
                .WithOne(m => m.Account)
                .HasForeignKey<Member>(m => m.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Member -> Vehicle relationship
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Member)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Member -> Battery relationship
            modelBuilder.Entity<Battery>()
                .HasOne(b => b.Member)
                .WithMany(m => m.Batteries)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Member -> Post relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Member)
                .WithMany(m => m.Posts)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Post -> Vehicle relationship (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.Posts)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Post -> Battery relationship (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Battery)
                .WithMany(b => b.Posts)
                .HasForeignKey(p => p.BatteryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure PostPackage -> PostPackageSub relationship
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.PostPackage)
                .WithMany(pp => pp.PostPackageSubs)
                .HasForeignKey(pps => pps.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Post -> PostPackageSub relationship
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Post)
                .WithMany(p => p.PostPackageSubs)
                .HasForeignKey(pps => pps.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Member -> PostPackageSub relationship
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Member)
                .WithMany(m => m.PostPackageSubs)
                .HasForeignKey(pps => pps.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Payment -> PostPackageSub relationship
            modelBuilder.Entity<PostPackageSub>()
                .HasOne(pps => pps.Payment)
                .WithMany(p => p.PostPackageSubs)
                .HasForeignKey(pps => pps.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ConstructFee -> ServiceFee relationship
            modelBuilder.Entity<ServiceFee>()
                .HasOne(sf => sf.ConstructFee)
                .WithOne(cf => cf.ServiceFee)
                .HasForeignKey<ServiceFee>(sf => sf.ConstructFeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Construct -> ConstructFee relationship
            modelBuilder.Entity<ConstructFee>()
                .HasOne(cf => cf.Construct)
                .WithMany(c => c.ConstructFees)
                .HasForeignKey(cf => cf.ConstructId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Member -> ConstructFee relationship
            modelBuilder.Entity<ConstructFee>()
                .HasOne(cf => cf.Member)
                .WithMany(m => m.ConstructFees)
                .HasForeignKey(cf => cf.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Payment -> Construct relationship
            modelBuilder.Entity<Construct>()
                .HasOne(c => c.Payment)
                .WithMany(p => p.Constructs)
                .HasForeignKey(c => c.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Member -> Payment relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Member)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PostRequest relationships
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
        }
    }
}
