using System;
using System.Threading.Tasks;

namespace EVehicleManagementAPI.Services
{
    public interface IOtpService
    {
        Task<string> CreateAndSendAsync(string email, string purpose, int? accountId = null, TimeSpan? ttl = null);
        Task<bool> VerifyAsync(string email, string code, string purpose);
    }
}


