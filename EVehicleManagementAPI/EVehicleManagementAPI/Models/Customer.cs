using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public int? AccountId { get; set; } // optional link to Account if applicable
        public Account Account { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Navigation
        public ICollection<Contracts> Contracts { get; set; } = new List<Contracts>();
    }
}