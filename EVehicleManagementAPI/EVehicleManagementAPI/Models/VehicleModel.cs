using System;
using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class VehicleModel
    {
        public int VehicleModelId { get; set; }
        public string Name { get; set; } = string.Empty;                    // Tên model (VD: "VF 8", "Model 3", "Atto 3")
        public string Brand { get; set; } = string.Empty;                   // Hãng (VD: "VinFast", "Tesla", "BYD")
        public int? Year { get; set; }                      // Năm sản xuất
        public string Type { get; set; } = string.Empty;                     // Loại xe (VD: "SUV", "Sedan", "Hatchback", "E-Bike")
        
        // Thông số kỹ thuật động cơ
        public decimal? MotorPower { get; set; }            // Công suất động cơ (kW)
        public string BatteryType { get; set; } = string.Empty;             // Loại pin (VD: "Lithium-ion", "LFP", "NMC")
        public decimal? Voltage { get; set; }               // Điện áp (V)
        
        // Thông số vận hành
        public int? MaxSpeed { get; set; }                  // Tốc độ tối đa (km/h)
        public int? Range { get; set; }                     // Quãng đường (km) theo chuẩn
        public decimal? Weight { get; set; }               // Trọng lượng (kg)
        public int? Seats { get; set; }                    // Số ghế
        
        // Thông tin bổ sung
        public string Description { get; set; } = string.Empty;            // Mô tả chi tiết
        public string? CustomSpec { get; set; }            // JSON cho các thuộc tính mở rộng (ít gặp)
        
        // Cờ đánh dấu
        public bool IsCustom { get; set; } = false;        // Model do user submit (chưa chuẩn hóa)
        public bool IsApproved { get; set; } = false;      // Đã được admin duyệt (nếu custom)
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
