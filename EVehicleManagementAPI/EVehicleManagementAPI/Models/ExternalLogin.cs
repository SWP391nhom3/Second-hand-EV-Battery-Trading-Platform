using System;

namespace EVehicleManagementAPI.Models
{
    public class ExternalLogin
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Provider { get; set; } // e.g., "Google"
        public string ProviderKey { get; set; } // e.g., Google sub (GoogleId)
        public DateTime CreatedAt { get; set; }

        public Account Account { get; set; }
    }
}


