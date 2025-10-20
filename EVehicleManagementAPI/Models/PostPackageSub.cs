using System;

namespace EVehicleManagementAPI.Models
{
    public class PostPackageSub
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int PackageId { get; set; }
        public int MemberId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PaymentId { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public PostPackage PostPackage { get; set; }
        public Member Member { get; set; }
        public Payment Payment { get; set; }
    }
}
