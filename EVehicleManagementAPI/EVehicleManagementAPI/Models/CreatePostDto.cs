namespace EVehicleManagementAPI.Models
{
    public class CreatePostDto
    {
        // Post fields
        public int MemberId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PostType { get; set; } = string.Empty;

        // Model selection (new flow)
        public int? VehicleModelId { get; set; }
        public int? BatteryModelId { get; set; }

        // Vehicle-specific fields (khi dùng VehicleModelId)
        public string? VehicleCondition { get; set; }
        public int? VehicleMileageKm { get; set; }

        // Battery-specific fields (khi dùng BatteryModelId)
        public string? BatteryCondition { get; set; }
        public int? BatteryCycleCount { get; set; }

        // Backward compatible: nhận Vehicle/Battery data trực tiếp (nếu không dùng model)
        public Vehicle? Vehicle { get; set; }
        public Battery? Battery { get; set; }
        
        // Backward compatible: nhận vehicleId/batteryId trực tiếp (để đảm bảo 100% tương thích)
        public int? VehicleId { get; set; }
        public int? BatteryId { get; set; }
    }
}

