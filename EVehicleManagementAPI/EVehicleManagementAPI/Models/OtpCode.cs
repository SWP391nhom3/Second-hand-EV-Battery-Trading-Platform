using System;

namespace EVehicleManagementAPI.Models
{
    public class OtpCode
    {
        public int Id { get; set; }
        public int? AccountId { get; set; } // nullable for pre-account registration
        public string Email { get; set; }
        public string Code { get; set; }
        public string Purpose { get; set; } // Register, Login, Link
        public DateTime ExpiresAt { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Account Account { get; set; }
    }
}


