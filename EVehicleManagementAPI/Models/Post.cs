using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int MemberId { get; set; }
        public int? VehicleId { get; set; }
        public int? BatteryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PostType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool Featured { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public Vehicle Vehicle { get; set; }
        public Battery Battery { get; set; }
        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
        public ICollection<PostRequest> PostRequests { get; set; } = new List<PostRequest>();
    }
}
