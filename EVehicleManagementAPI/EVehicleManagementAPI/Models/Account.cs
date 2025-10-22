using System;

namespace EVehicleManagementAPI.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public Member Member { get; set; }
    }
}
