namespace EVehicleManagementAPI.Models.Requests
{
    public class ChangePasswordRequest
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
