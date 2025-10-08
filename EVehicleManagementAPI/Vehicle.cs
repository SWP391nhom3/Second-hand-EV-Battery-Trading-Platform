namespace EVehicleManagementAPI.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Car, Bike
        public decimal Price { get; set; }
        public string Status { get; set; }
    }
}