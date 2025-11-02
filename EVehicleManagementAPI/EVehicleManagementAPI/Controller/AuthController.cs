using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using EVehicleManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EVehicleDbContext _context;
        private readonly IGoogleOAuthService _google;
        private readonly IOtpService _otpService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        // Simple in-memory pending store for Google register flow
        private static readonly ConcurrentDictionary<string, PendingExternal> _pending = new ConcurrentDictionary<string, PendingExternal>();

        public AuthController(EVehicleDbContext context, IGoogleOAuthService google, IOtpService otpService, ITokenService tokenService, IConfiguration config)
        {
            _context = context;
            _google = google;
            _otpService = otpService;
            _tokenService = tokenService;
            _config = config;
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

            var token = _tokenService.CreateJwt(account.AccountId, account.Email, account.Role?.Name);
            account.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                account = new
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

            var token = _tokenService.CreateJwt(account.AccountId, account.Email, account.Role?.Name);
            account.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                account = new
            {
                accountId = account.AccountId,
                email = account.Email,
                role = account.Role?.Name
                }
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

        // ==== GOOGLE OAUTH ====
        [HttpGet("google/start")]
        public IActionResult GoogleStart([FromQuery] string state)
        {
            state ??= Guid.NewGuid().ToString("N");
            var url = _google.BuildAuthorizationUrl(state);
            return Ok(new { url, state });
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string redirectUri)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest(new { message = "Missing code" });
            var configuredRedirect = _config["Authentication:Google:RedirectUri"];
            var effectiveRedirect = string.IsNullOrWhiteSpace(configuredRedirect) ? redirectUri : configuredRedirect;
            var user = await _google.ExchangeCodeForUserAsync(code, effectiveRedirect);
            if (user == null || string.IsNullOrEmpty(user.Email)) return BadRequest(new { message = "Unable to retrieve Google user" });

            var account = await _context.Accounts.Include(a => a.Member).FirstOrDefaultAsync(a => a.Email == user.Email);
            if (account != null)
            {
                if (string.IsNullOrEmpty(account.GoogleId))
                {
                    account.GoogleId = user.Sub;
                    await _context.SaveChangesAsync();
                }
                if (!account.EmailVerified)
                {
                    // Nếu quá 1 phút mà chưa xác minh -> xoá account để đăng ký lại
                    if (DateTime.Now - account.CreatedAt > TimeSpan.FromMinutes(1))
                    {
                        var accountId = account.AccountId;
                        var emailToRemove = account.Email;
                        var member = await _context.Members.FirstOrDefaultAsync(m => m.AccountId == accountId);
                        if (member != null) _context.Members.Remove(member);
                        var externalLogins = _context.Set<ExternalLogin>().Where(x => x.AccountId == accountId);
                        _context.RemoveRange(externalLogins);
                        var otps = _context.Set<OtpCode>().Where(o => o.AccountId == accountId || o.Email == emailToRemove);
                        _context.RemoveRange(otps);
                        _context.Accounts.Remove(account);
                        await _context.SaveChangesAsync();

                        // Trả về chế độ đăng ký lại với pendingToken
                        var newPendingToken = Guid.NewGuid().ToString("N");
                        _pending[newPendingToken] = new PendingExternal
                        {
                            Email = user.Email,
                            FullName = user.Name,
                            AvatarUrl = user.Picture,
                            GoogleId = user.Sub
                        };
                        return Ok(new { mode = "register", pendingToken = newPendingToken, profile = new { email = user.Email, name = user.Name, avatar = user.Picture } });
                    }

                    // account chưa xác minh và còn hạn -> gửi OTP đăng ký
                    await _otpService.CreateAndSendAsync(account.Email, "Register", account.AccountId);
                    return Ok(new { mode = "register", email = account.Email, requiresOtp = true });
                }
                // Đã xác minh -> login trực tiếp, không OTP (OTP chỉ khi bật 2FA)
                var token = _tokenService.CreateJwt(account.AccountId, account.Email, account.Role?.Name);
                account.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new { mode = "login", token });
            }

            // register mode -> store pending and return profile
            var pendingToken = Guid.NewGuid().ToString("N");
            _pending[pendingToken] = new PendingExternal
            {
                Email = user.Email,
                FullName = user.Name,
                AvatarUrl = user.Picture,
                GoogleId = user.Sub
            };
            return Ok(new { mode = "register", pendingToken, profile = new { email = user.Email, name = user.Name, avatar = user.Picture } });
        }

        public class GoogleRegisterCompleteRequest
        {
            public string PendingToken { get; set; }
            public string FullName { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("google/register-complete")]
        public async Task<IActionResult> GoogleRegisterComplete([FromBody] GoogleRegisterCompleteRequest req)
        {
            if (string.IsNullOrEmpty(req?.PendingToken) || !_pending.TryRemove(req.PendingToken, out var p))
            {
                return BadRequest(new { message = "Invalid or expired pending token" });
            }

            if (await _context.Accounts.AnyAsync(a => a.Email == p.Email))
            {
                return Conflict(new { message = "Email already exists" });
            }

            var account = new Account
            {
                Email = p.Email,
                Phone = "",
                PasswordHash = HashPassword(req.Password),
                RoleId = 2,
                CreatedAt = DateTime.Now,
                GoogleId = p.GoogleId,
                EmailVerified = false
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var member = new Member
            {
                AccountId = account.AccountId,
                FullName = string.IsNullOrWhiteSpace(req.FullName) ? p.FullName : req.FullName,
                AvatarUrl = p.AvatarUrl ?? "",
                Address = "",
                JoinedAt = DateTime.Now,
                Rating = 0,
                Status = "ACTIVE"
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            await _otpService.CreateAndSendAsync(account.Email, "Register", account.AccountId);
            return Ok(new { message = "OTP sent to email for registration", email = account.Email });
        }

        public class GoogleLoginOtpRequest
        {
            public string Email { get; set; }
        }

        [HttpPost("google/login-otp")]
        public async Task<IActionResult> GoogleLoginOtp([FromBody] GoogleLoginOtpRequest req)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == req.Email);
            if (account == null) return NotFound(new { message = "Account not found" });
            await _otpService.CreateAndSendAsync(account.Email, "Login", account.AccountId);
            return Ok(new { message = "OTP sent", email = account.Email });
        }

        public class OtpVerifyRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
            public string Purpose { get; set; } // Register | Login
        }

        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest req)
        {
            var ok = await _otpService.VerifyAsync(req.Email, req.Code, req.Purpose);
            if (!ok) return BadRequest(new { message = "Invalid or expired OTP" });

            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.Email == req.Email);
            if (account == null) return NotFound(new { message = "Account not found" });

            if (string.Equals(req.Purpose, "Register", StringComparison.OrdinalIgnoreCase))
            {
                account.EmailVerified = true;
                await _context.SaveChangesAsync();
            }

            account.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateJwt(account.AccountId, account.Email, account.Role?.Name);
            return Ok(new { token });
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

namespace EVehicleManagementAPI.Controllers
{
    public class PendingExternal
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string GoogleId { get; set; }
    }
}
