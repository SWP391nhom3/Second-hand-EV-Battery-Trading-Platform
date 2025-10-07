namespace EVehicleManagementAPI.Models
{
    // Join table between Account and Role
    public class AccountRole
    {
        public int AccountRoleId { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}