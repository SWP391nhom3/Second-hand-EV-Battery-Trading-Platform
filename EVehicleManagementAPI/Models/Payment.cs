using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string TransferContent { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public ICollection<Construct> Constructs { get; set; } = new List<Construct>();
        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
    }
}