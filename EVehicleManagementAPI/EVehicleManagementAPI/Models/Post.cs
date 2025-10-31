using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVehicleManagementAPI.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int MemberId { get; set; }              // người đăng bài
        public int? VehicleId { get; set; }
        public int? BatteryId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        // ✅ Loại bài đăng: E-Vehicle / E-Bike / Battery
        public string PostType { get; set; }

        // ✅ Kiểu giao dịch: DIRECT (tự liên hệ) | STAFF_ASSISTED (có nhân viên hỗ trợ)
        public string TransactionType { get; set; }

        // ✅ Nhân viên được admin gán (nếu có)
        public int? StaffId { get; set; }

        // ✅ Thông tin liên hệ trực tiếp (nếu là DIRECT)
        public string? ContactInfo { get; set; }

        public string Status { get; set; }             // ACTIVE, SOLD, EXPIRED,...
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public bool Featured { get; set; }

        // 🔗 Navigation properties
        public Member Member { get; set; }
        public Vehicle Vehicle { get; set; }
        public Battery Battery { get; set; }

        [ForeignKey("StaffId")]
        public Member? Staff { get; set; }             // nhân viên hỗ trợ

        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
        public ICollection<PostRequest> PostRequests { get; set; } = new List<PostRequest>();
    }
}
