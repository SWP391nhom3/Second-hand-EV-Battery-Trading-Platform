using System;

namespace EVehicleManagementAPI.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int ContractsId { get; set; }
        public Contracts Contracts { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }

        public string Method { get; set; } // e.g., Card, Cash
    }
}