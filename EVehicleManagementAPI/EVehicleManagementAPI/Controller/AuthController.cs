using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
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
        private readonly EVehicleDbContext _context;

        public AuthController(EVehicleDbContext context)
        {
            _context = context;
        }

        // 🔹 Đăng ký tài khoản mới
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Account account)
        {
            // Kiểm tra trùng Email hoặc Phone
            var existingUser = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Email == account.Email || u.Phone == account.Phone);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email hoặc số điện thoại đã được đăng ký." });
            }

            // Mã hóa mật khẩu (SHA256)
            account.PasswordHash = HashPassword(account.PasswordHash);
            account.CreatedAt = DateTime.Now;

            // Gán role mặc định (nếu có)
            if (account.RoleId == 0)
            {
                account.RoleId = 2; // ví dụ: 1=Admin, 2=Member
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", accountId = account.AccountId });
        }

        // 🔹 Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Email == request.EmailOrPhone || u.Phone == request.EmailOrPhone);

            if (account == null)
            {
                return BadRequest(new { message = "Tài khoản không tồn tại." });
            }

            var hashedInputPassword = HashPassword(request.Password);
            if (account.PasswordHash != hashedInputPassword)
            {
                return BadRequest(new { message = "Sai mật khẩu." });
            }

            // Ở đây bạn có thể thêm JWT Token (sau này)
            return Ok(new
            {
                message = "Đăng nhập thành công",
                accountId = account.AccountId,
                email = account.Email,
                roleId = account.RoleId
            });
        }

        // 🔒 Hàm hash mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    // 🔹 Model phụ cho Login
    public class LoginRequest
    {
        public string? EmailOrPhone { get; set; }
        public string? Password { get; set; }
    }
}
