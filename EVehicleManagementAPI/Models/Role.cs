using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}