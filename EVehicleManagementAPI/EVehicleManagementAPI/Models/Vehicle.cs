using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int? VehicleModelId { get; set; }  // Foreign key to VehicleModel (optional)
        public string VIN { get; set; }
        public int ManufactureYear { get; set; }
        public int MileageKm { get; set; }
        public decimal BatteryCapacity { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public VehicleModel? VehicleModel { get; set; }  // Reference to VehicleModel
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
