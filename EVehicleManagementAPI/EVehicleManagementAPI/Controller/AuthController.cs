using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using EVehicleManagementAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EVehicleDbContext _dbContext;

        public AuthController(EVehicleDbContext context)
        {
            _dbContext = context;
        }

        // 🧾 REGISTER — chỉ cho Member được đăng ký
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _dbContext.Accounts.AnyAsync(a => a.Email == request.Email))
                return BadRequest(new { message = "❌ Email already exists" });

            var memberRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
            if (memberRole == null)
                return BadRequest(new { message = "❌ Member role not found" });

            // 🔹 Hash mật khẩu bằng SHA256 (định dạng Hex, trùng với DB)
            var passwordHash = HashPasswordInternal(request.Password);

            var account = new Account
            {
                Email = request.Email,
                Phone = "",
                PasswordHash = passwordHash,
                RoleId = memberRole.RoleId,
                CreatedAt = DateTime.Now
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            var member = new Member
            {
                AccountId = account.AccountId,
                FullName = request.FullName,
                AvatarUrl = "",
                Address = "",
                JoinedAt = DateTime.Now,
                Rating = 0,
                Status = "ACTIVE"
            };

            _dbContext.Members.Add(member);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "✅ Registration successful", accountId = account.AccountId });
        }

        // 🔐 LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _dbContext.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            // 🔹 Kiểm tra email và password
            if (account == null || !VerifyPasswordInternal(request.Password, account.PasswordHash))
                return Unauthorized(new { message = "❌ Invalid email or password" });

            return Ok(new
            {
                accountId = account.AccountId,
                email = account.Email,
                role = account.Role?.Name,
                member = account.Member == null ? null : new
                {
                    account.Member.FullName,
                    account.Member.AvatarUrl,
                    account.Member.Rating
                }
            });
        }

        // 🔑 CHANGE PASSWORD
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var account = await _dbContext.Accounts.FindAsync(request.AccountId);
            if (account == null)
                return NotFound(new { message = "❌ Account not found" });

            if (!VerifyPasswordInternal(request.OldPassword, account.PasswordHash))
                return BadRequest(new { message = "⚠️ Current password is incorrect" });

            account.PasswordHash = HashPasswordInternal(request.NewPassword);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "✅ Password changed successfully" });
        }

        // 🔄 FORGOT PASSWORD
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
            if (account == null)
                return NotFound(new { message = "❌ Email not found" });

            return Ok(new { message = "📩 Password reset instructions sent to your email" });
        }

        // 📦 HASH + VERIFY
        private string HashPasswordInternal(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // ⚙️ Định dạng HEX (trùng DB)
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        private bool VerifyPasswordInternal(string password, string hashedPassword)
        {
            return HashPasswordInternal(password) == hashedPassword;
        }
    }
}
