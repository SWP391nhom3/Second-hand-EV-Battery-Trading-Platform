using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Services
{
    public class OtpService : IOtpService
    {
        private readonly EVehicleDbContext _db;
        private readonly IEmailService _email;

        public OtpService(EVehicleDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task<string> CreateAndSendAsync(string email, string purpose, int? accountId = null, TimeSpan? ttl = null)
        {
            var code = GenerateCode();
            var now = DateTime.UtcNow;

            // simple throttling: max 3 OTPs per 1 minute per email
            var windowStart = now.AddMinutes(-1);
            var recentCount = await _db.OtpCodes.CountAsync(o => o.Email == email && o.CreatedAt >= windowStart);
            if (recentCount >= 3)
            {
                throw new InvalidOperationException("Too many OTP requests. Please try again later.");
            }
            var otp = new OtpCode
            {
                AccountId = accountId,
                Email = email,
                Code = code,
                Purpose = purpose,
                CreatedAt = now,
                ExpiresAt = now.Add(ttl ?? TimeSpan.FromMinutes(5))
            };
            _db.OtpCodes.Add(otp);
            await _db.SaveChangesAsync();

            await _email.SendOtpAsync(email, code, purpose);
            return code;
        }

        public async Task<bool> VerifyAsync(string email, string code, string purpose)
        {
            var now = DateTime.UtcNow;
            var otp = await _db.OtpCodes
                .Where(o => o.Email == email && o.Purpose == purpose && o.ConsumedAt == null)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null) return false;
            if (!string.Equals(otp.Code, code, StringComparison.Ordinal)) return false;
            if (otp.ExpiresAt < now) return false;

            otp.ConsumedAt = now;
            await _db.SaveChangesAsync();
            return true;
        }

        private static string GenerateCode()
        {
            // 6-digit numeric OTP
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1000000u;
            return value.ToString("D6");
        }
    }
}


