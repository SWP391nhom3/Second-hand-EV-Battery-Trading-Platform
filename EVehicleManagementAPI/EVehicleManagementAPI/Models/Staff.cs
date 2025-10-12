using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Navigation
        public ICollection<Contracts> ContractsHandled { get; set; } = new List<Contracts>();
    }
}