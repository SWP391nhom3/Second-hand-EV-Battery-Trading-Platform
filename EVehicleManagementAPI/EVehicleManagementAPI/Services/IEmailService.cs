using System.Threading.Tasks;

namespace EVehicleManagementAPI.Services
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string code, string purpose);
    }
}


