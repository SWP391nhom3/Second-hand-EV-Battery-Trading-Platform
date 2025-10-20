using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Address { get; set; }
        public DateTime JoinedAt { get; set; }
        public decimal Rating { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public Account Account { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Battery> Batteries { get; set; } = new List<Battery>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<ConstructFee> ConstructFees { get; set; } = new List<ConstructFee>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
        public ICollection<PostRequest> PostRequests { get; set; } = new List<PostRequest>();
    }
}
