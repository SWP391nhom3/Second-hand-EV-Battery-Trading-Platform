using System;

namespace EVehicleManagementAPI.Models
{
    public class PostRequest
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int BuyerId { get; set; }
        public int? ConstructId { get; set; }
        public string Message { get; set; }
        public decimal OfferPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public Member Buyer { get; set; }
        public Construct Construct { get; set; }
    }
}
