using System;

namespace EVehicleManagementAPI.Models
{
    public class ViewHistory
    {
        public int ViewHistoryId { get; set; }

        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public DateTime ViewedAt { get; set; }
        public string? Meta { get; set; } // optional extra info (e.g., IP, user agent)
    }
}