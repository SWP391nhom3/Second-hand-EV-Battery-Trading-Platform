using System.Threading.Tasks;

namespace EVehicleManagementAPI.Services
{
    public class GoogleUserInfo
    {
        public string Sub { get; set; } // GoogleId
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }

    public interface IGoogleOAuthService
    {
        string BuildAuthorizationUrl(string state);
        Task<GoogleUserInfo> ExchangeCodeForUserAsync(string code, string redirectUri);
    }
}


