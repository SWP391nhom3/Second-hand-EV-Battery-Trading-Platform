using System;
using System.Collections.Generic;

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

        // OAuth/Verification
        public string? GoogleId { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public Member Member { get; set; }
        public ICollection<ExternalLogin> ExternalLogins { get; set; }
        public ICollection<OtpCode> OtpCodes { get; set; }
    }
}
