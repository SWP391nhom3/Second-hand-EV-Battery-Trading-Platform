namespace EVehicleManagementAPI.Models
{
    public class Admin
    {
        public int AdminId { get; set; }
        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
    }
}