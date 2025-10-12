namespace EVehicleManagementAPI.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }          // Khóa chính
        public string Name { get; set; }            // Tên xe
        public string Type { get; set; }            // Loại xe: Car, Bike,...
        public decimal Price { get; set; }          // Giá bán hoặc cho thuê
        public string Status { get; set; }          // Trạng thái: Available, Sold, etc.
    }
}
