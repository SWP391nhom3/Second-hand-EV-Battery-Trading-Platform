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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            
            // Check if email already exists
            if (await _context.Accounts.AnyAsync(a => a.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            /* Commented out phone check for now
            // Check if phone already exists
            if (await _context.Accounts.AnyAsync(a => a.Phone == request.Phone))
            {
                return BadRequest(new { message = "Phone number already exists" });
            }
            */
            // Hash password
            var passwordHash = HashPassword(request.Password);

            // Create account
            var account = new Account
            {
                Email = request.Email,
                Phone = "", // Empty phone - can be updated later
                PasswordHash = passwordHash,
                RoleId = 2, // Default role for members (assuming 1=Admin, 2=Member)
                CreatedAt = DateTime.Now
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Create member profile with minimal required fields
            var member = new Member
            {
                AccountId = account.AccountId,
                FullName = request.FullName,
                AvatarUrl = "", // Empty avatar - can be updated later
                Address = "", // Empty address - can be updated later
                JoinedAt = DateTime.Now,
                Rating = 0,
                Status = "ACTIVE"
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful", accountId = account.AccountId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            if (account == null || !VerifyPassword(request.Password, account.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (account.Member?.Status != "ACTIVE")
            {
                return Unauthorized(new { message = "Account is not active" });
            }

            return Ok(new
            {
                accountId = account.AccountId,
                email = account.Email,
                phone = account.Phone,
                role = account.Role?.Name,
                member = new
                {
                    memberId = account.Member?.MemberId,
                    fullName = account.Member?.FullName,
                    avatarUrl = account.Member?.AvatarUrl,
                    rating = account.Member?.Rating
                }
            });
        }

        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin([FromBody] LoginRequest request)
        {
            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            if (account == null || !VerifyPassword(request.Password, account.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!string.Equals(account.Role?.Name, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return Ok(new
            {
                accountId = account.AccountId,
                email = account.Email,
                role = account.Role?.Name
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null)
            {
                return NotFound(new { message = "Account not found" });
            }

            if (!VerifyPassword(request.OldPassword, account.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            account.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
            if (account == null)
            {
                return NotFound(new { message = "Email not found" });
            }

            // In a real application, you would send a reset link via email
            // For now, we'll just return a success message
            return Ok(new { message = "Password reset instructions sent to your email" });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
}
