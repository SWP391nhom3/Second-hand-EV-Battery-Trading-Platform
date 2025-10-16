namespace EVehicleManagementAPI.Models
{
    public class ServiceFee
    {
        public int Id { get; set; }
        public int ConstructFeeId { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; } = "ACTIVE";

        // Navigation properties
        public ConstructFee ConstructFee { get; set; }
    }
}
