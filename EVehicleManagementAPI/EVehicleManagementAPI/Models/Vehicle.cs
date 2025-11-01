using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }      // 🔹 Khóa chính
        public int MemberId { get; set; }       // 🔹 Khóa ngoại - người sở hữu xe

        // 🔹 Thông tin cơ bản
        public string Brand { get; set; }       // Hãng xe (VinFast, Yadea,...)
        public string Model { get; set; }       // Tên mẫu xe (VF3, Klara, E3,...)
        public int ManufactureYear { get; set; }
        public int MileageKm { get; set; }
        public decimal BatteryCapacity { get; set; }  // Dung lượng pin (kWh)
        public string Condition { get; set; }   // Mới / Đã qua sử dụng
        public string Description { get; set; }
        public decimal Price { get; set; }      // Giá xe
        public string Status { get; set; } = "AVAILABLE";  // AVAILABLE | SOLD | INACTIVE
        public string ImageUrl { get; set; }    // Ảnh xe (URL)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 Navigation properties
        public Member Member { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
