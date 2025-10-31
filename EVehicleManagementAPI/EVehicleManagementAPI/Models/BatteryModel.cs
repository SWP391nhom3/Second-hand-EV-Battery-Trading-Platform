using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class BatteryModel
    {
        public int BatteryModelId { get; set; }
        public string Name { get; set; } = string.Empty;                    // Tên model (VD: "CATL 100kWh", "Panasonic 2170", "VinES 42kWh")
        public string Brand { get; set; } = string.Empty;                   // Hãng (VD: "CATL", "Panasonic", "VinES", "BYD")
        
        // Thông số kỹ thuật pin
        public string Chemistry { get; set; } = string.Empty;             // Hóa học pin (VD: "NCM811", "LFP", "NCA")
        public decimal? Voltage { get; set; }             // Điện áp (V)
        public decimal? CapacityKWh { get; set; }         // Dung lượng (kWh)
        public decimal? Amperage { get; set; }           // Dòng điện (A)
        
        // Thông số vật lý
        public decimal? Weight { get; set; }              // Trọng lượng (kg)
        public string FormFactor { get; set; } = string.Empty;           // Hình dạng/kích thước (VD: "Prismatic", "Cylindrical", "Pouch")
        
        // Thông số tuổi thọ
        public int? Cycles { get; set; }                  // Số chu kỳ sạc (theo spec nhà sản xuất)
        
        // Thông tin bổ sung
        public string Description { get; set; } = string.Empty;          // Mô tả chi tiết
        public string? CustomSpec { get; set; }          // JSON cho các thuộc tính mở rộng
        
        // Cờ đánh dấu
        public bool IsCustom { get; set; } = false;      // Model do user submit (chưa chuẩn hóa)
        public bool IsApproved { get; set; } = false;    // Đã được admin duyệt (nếu custom)
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Battery> Batteries { get; set; } = new List<Battery>();
    }
}
