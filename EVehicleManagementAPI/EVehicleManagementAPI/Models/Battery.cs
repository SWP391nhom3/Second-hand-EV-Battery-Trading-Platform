using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Battery
    {
        public int BatteryId { get; set; }
        public int MemberId { get; set; }
        
        // Foreign key đến BatteryModel (nullable - cho phép nhập thủ công)
        public int? BatteryModelId { get; set; }
        
        // Các field cũ (giữ lại để backward compatible với dữ liệu cũ)
        public string Brand { get; set; }
        public decimal CapacityKWh { get; set; }
        public int CycleCount { get; set; }
        public int ManufactureYear { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public BatteryModel BatteryModel { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
