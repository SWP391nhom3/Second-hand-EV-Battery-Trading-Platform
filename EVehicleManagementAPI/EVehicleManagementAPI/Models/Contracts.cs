using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    // Named 'Contracts' to match the DbContext declaration (DbSet<Contracts> Contracts)
    public class Contracts
    {
        public int ContractsId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int? StaffId { get; set; }
        public Staff Staff { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        // Navigation
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}