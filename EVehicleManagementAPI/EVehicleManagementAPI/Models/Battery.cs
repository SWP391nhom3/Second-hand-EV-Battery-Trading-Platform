using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class Battery
    {
        public int BatteryId { get; set; }
        public int MemberId { get; set; }
        public int? BatteryModelId { get; set; }  // Foreign key to BatteryModel (optional)
        public decimal CapacityKWh { get; set; }
        public int CycleCount { get; set; }
        public int ManufactureYear { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public Member Member { get; set; }
        public BatteryModel? BatteryModel { get; set; }  // Reference to BatteryModel
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
