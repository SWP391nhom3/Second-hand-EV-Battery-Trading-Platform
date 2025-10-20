using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Construct
    {
        public int ConstructId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Type { get; set; }
        public int PaymentId { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public Payment Payment { get; set; }
        public ICollection<ConstructFee> ConstructFees { get; set; } = new List<ConstructFee>();
        public ICollection<PostRequest> PostRequests { get; set; } = new List<PostRequest>();
    }
}
