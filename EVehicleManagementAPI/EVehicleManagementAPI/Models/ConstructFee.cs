using System;

namespace EVehicleManagementAPI.Models
{
    public class ConstructFee
    {
        public int Id { get; set; }
        public int ConstructId { get; set; }
        public int MemberId { get; set; }
        public string ServiceName { get; set; }
        public decimal Fee { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Construct Construct { get; set; }
        public Member Member { get; set; }
        public ServiceFee ServiceFee { get; set; }
    }
}
