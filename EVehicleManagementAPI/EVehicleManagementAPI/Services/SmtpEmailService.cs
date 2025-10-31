using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EVehicleManagementAPI.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string code, string purpose)
        {
            var host = _config["Email:Smtp:Host"];
            var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
            var user = _config["Email:Smtp:User"];
            var pass = _config["Email:Smtp:Password"];
            var from = _config["Email:Smtp:From"] ?? user;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass)
            };

            var subject = $"Your OTP for {purpose}";
            var body = $"Your OTP code is: {code}. It will expire in 5 minutes.";
            using var mail = new MailMessage(from, toEmail, subject, body);
            await client.SendMailAsync(mail);
        }
    }
}


