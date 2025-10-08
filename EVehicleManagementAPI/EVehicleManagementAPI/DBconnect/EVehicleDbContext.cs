using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Contracts> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ViewHistory> ViewHistories { get; set; }
    }
}
