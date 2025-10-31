using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        
        // Foreign key đến VehicleModel (nullable - cho phép nhập thủ công)
        public int? VehicleModelId { get; set; }
        
        // Các field cũ (giữ lại để backward compatible với dữ liệu cũ)
        public string Brand { get; set; }
        public string Model { get; set; }
        public int ManufactureYear { get; set; }
        public int MileageKm { get; set; }
        public decimal BatteryCapacity { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public VehicleModel VehicleModel { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
