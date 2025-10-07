using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        // Navigation
        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
    }
}