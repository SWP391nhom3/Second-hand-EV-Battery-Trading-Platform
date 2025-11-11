using EVehicle.Application.DTOs.Auth;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Service xử lý các nghiệp vụ Authentication (UC01, UC02, UC03)
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ISocialAuthService _socialAuthService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ISocialAuthService socialAuthService,
        IEmailVerificationService emailVerificationService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _socialAuthService = socialAuthService;
        _emailVerificationService = emailVerificationService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// UC01: Đăng ký tài khoản mới
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Bắt đầu đăng ký tài khoản cho Email: {Email}", request.Email);

        // Kiểm tra Email đã tồn tại
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Email đã tồn tại: {Email}", request.Email);
            throw new InvalidOperationException("Email đã được sử dụng");
        }

        // Kiểm tra Phone đã tồn tại
        if (await _userRepository.PhoneNumberExistsAsync(request.PhoneNumber))
        {
            _logger.LogWarning("Số điện thoại đã tồn tại: {PhoneNumber}", request.PhoneNumber);
            throw new InvalidOperationException("Số điện thoại đã được sử dụng");
        }

        // Tạo user mới với trạng thái PENDING_VERIFICATION
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName?.Trim(),
            Role = "MEMBER",
            Status = "PENDING_VERIFICATION", // Tài khoản chưa verify email
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        // Lưu vào database
        var createdUser = await _userRepository.CreateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Đăng ký tài khoản thành công cho UserId: {UserId}, Status: {Status}", 
            createdUser.Id, createdUser.Status);

        // Gửi OTP để verify email
        try
        {
            await _emailVerificationService.GenerateAndSendOtpAsync(
                createdUser.Email, 
                createdUser.FullName ?? "");
            _logger.LogInformation("Đã gửi OTP verify email cho UserId: {UserId}", createdUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi OTP verify email cho UserId: {UserId}", createdUser.Id);
            // Vẫn tạo tài khoản nhưng báo lỗi về việc gửi email
            // User có thể dùng resend-otp endpoint để gửi lại OTP
            return new AuthResponse
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
                FullName = createdUser.FullName,
                Role = createdUser.Role,
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                AccessTokenExpiry = null,
                RefreshTokenExpiry = null,
                Message = "Đăng ký thành công nhưng không thể gửi email xác nhận. Vui lòng sử dụng chức năng 'Gửi lại OTP' để nhận mã xác nhận."
            };
        }

        // Không tạo token ngay, user cần verify email trước
        // Trả về response với message yêu cầu verify email
        return new AuthResponse
        {
            UserId = createdUser.Id,
            Email = createdUser.Email,
            PhoneNumber = createdUser.PhoneNumber,
            FullName = createdUser.FullName,
            Role = createdUser.Role,
            AccessToken = string.Empty, // Không có token vì chưa verify email
            RefreshToken = string.Empty,
            AccessTokenExpiry = null,
            RefreshTokenExpiry = null,
            Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản."
        };
    }

    /// <summary>
    /// UC02: Đăng nhập bằng Email/Phone và Password
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Bắt đầu đăng nhập cho: {EmailOrPhone}", request.EmailOrPhone);

        var emailOrPhone = request.EmailOrPhone.ToLower().Trim();
        
        // Tìm user theo Email hoặc Phone
        User? user = null;
        
        if (emailOrPhone.Contains('@'))
        {
            // Đăng nhập bằng Email
            user = await _userRepository.GetByEmailAsync(emailOrPhone);
        }
        else
        {
            // Đăng nhập bằng Phone
            user = await _userRepository.GetByPhoneNumberAsync(emailOrPhone);
        }

        // Kiểm tra user tồn tại
        if (user == null)
        {
            _logger.LogWarning("Không tìm thấy user với Email/Phone: {EmailOrPhone}", emailOrPhone);
            throw new UnauthorizedAccessException("Email/Số điện thoại hoặc mật khẩu không đúng");
        }

        // Kiểm tra password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Mật khẩu không đúng cho UserId: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Email/Số điện thoại hoặc mật khẩu không đúng");
        }

        // Kiểm tra email đã được verify chưa
        if (!user.EmailVerified)
        {
            _logger.LogWarning("Email chưa được verify, UserId: {UserId}", user.Id);
            throw new UnauthorizedAccessException(
                "Email chưa được xác nhận. Vui lòng kiểm tra email và xác nhận tài khoản trước khi đăng nhập.");
        }

        // Kiểm tra trạng thái tài khoản
        if (user.Status != "ACTIVE")
        {
            _logger.LogWarning("Tài khoản không hoạt động, Status: {Status}, UserId: {UserId}", 
                user.Status, user.Id);
            throw new UnauthorizedAccessException($"Tài khoản đang bị {user.Status}");
        }

        _logger.LogInformation("Đăng nhập thành công cho UserId: {UserId}", user.Id);

        // Tạo Access Token và Refresh Token
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }

    /// <summary>
    /// UC03: Đăng nhập bằng mạng xã hội (Google, Facebook)
    /// </summary>
    public async Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request)
    {
        var provider = request.Provider.ToLower();
        _logger.LogInformation("Bắt đầu đăng nhập bằng {Provider}", provider);

        // Xác thực token với provider và lấy thông tin user
        var (email, fullName, socialLoginId) = await _socialAuthService
            .ValidateAndGetUserInfoAsync(provider, request.Token);

        _logger.LogInformation("Xác thực token thành công, Email: {Email}, Provider: {Provider}", 
            email, provider);

        // Tìm user đã liên kết với social account
        var user = await _userRepository.GetBySocialLoginAsync(provider, socialLoginId);

        if (user == null)
        {
            // Kiểm tra email đã tồn tại chưa
            user = await _userRepository.GetByEmailAsync(email);

            if (user != null)
            {
                // Liên kết social account với tài khoản hiện có
                user.SocialLoginProvider = provider;
                user.SocialLoginId = socialLoginId;
                // Nếu email chưa được verify, tự động verify khi liên kết social account
                if (!user.EmailVerified)
                {
                    user.EmailVerified = true;
                    user.EmailVerifiedAt = DateTime.UtcNow;
                    user.Status = "ACTIVE"; // Kích hoạt tài khoản
                }
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Liên kết social account với UserId: {UserId}", user.Id);
            }
            else
            {
                // Tạo tài khoản mới từ social account
                // Social login đã được verify bởi provider nên không cần verify email
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email.ToLower().Trim(),
                    PhoneNumber = string.Empty, // Sẽ được cập nhật sau
                    PasswordHash = string.Empty, // Không cần password cho social login
                    FullName = fullName?.Trim(),
                    SocialLoginProvider = provider,
                    SocialLoginId = socialLoginId,
                    Role = "MEMBER",
                    Status = "ACTIVE",
                    EmailVerified = true, // Social login đã được verify
                    EmailVerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                user = await _userRepository.CreateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Tạo tài khoản mới từ social login, UserId: {UserId}", user.Id);
            }
        }

        // Kiểm tra trạng thái tài khoản
        if (user.Status != "ACTIVE")
        {
            _logger.LogWarning("Tài khoản không hoạt động, Status: {Status}, UserId: {UserId}", 
                user.Status, user.Id);
            throw new UnauthorizedAccessException($"Tài khoản đang bị {user.Status}");
        }

        _logger.LogInformation("Đăng nhập social thành công cho UserId: {UserId}", user.Id);

        // Tạo Access Token và Refresh Token
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }

    /// <summary>
    /// Xác thực JWT token và lấy thông tin user
    /// </summary>
    public async Task<UserDto?> ValidateTokenAsync(string token)
    {
        try
        {
            var claimsPrincipal = _jwtService.ValidateToken(token);
            if (claimsPrincipal == null)
                return null;

            var userIdClaim = claimsPrincipal.FindFirst("sub") ?? claimsPrincipal.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != "ACTIVE")
                return null;

            return new UserDto
            {
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi validate token");
            return null;
        }
    }

    /// <summary>
    /// Refresh Access Token và Refresh Token
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Bắt đầu refresh token");

        // Validate refresh token
        var claimsPrincipal = _jwtService.ValidateRefreshToken(refreshToken);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Refresh token không hợp lệ hoặc đã hết hạn");
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
        }

        // Lấy UserId từ token
        var userIdClaim = claimsPrincipal.FindFirst("sub") ?? claimsPrincipal.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Không tìm thấy UserId trong refresh token");
            throw new UnauthorizedAccessException("Refresh token không hợp lệ");
        }

        // Lấy thông tin user từ database
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Không tìm thấy user với UserId: {UserId}", userId);
            throw new UnauthorizedAccessException("Refresh token không hợp lệ");
        }

        // Kiểm tra trạng thái tài khoản
        if (user.Status != "ACTIVE")
        {
            _logger.LogWarning("Tài khoản không hoạt động, Status: {Status}, UserId: {UserId}", 
                user.Status, user.Id);
            throw new UnauthorizedAccessException($"Tài khoản đang bị {user.Status}");
        }

        _logger.LogInformation("Refresh token thành công cho UserId: {UserId}", user.Id);

        // Tạo Access Token và Refresh Token mới
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }

    /// <summary>
    /// Verify email với OTP code
    /// </summary>
    public async Task<VerifyEmailResponse> VerifyEmailAsync(string email, string otpCode)
    {
        _logger.LogInformation("Bắt đầu verify email: {Email}", email);

        var normalizedEmail = email.ToLower().Trim();

        // Tìm user theo email
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
        {
            _logger.LogWarning("Không tìm thấy user với email: {Email}", email);
            return new VerifyEmailResponse
            {
                Success = false,
                Message = "Email không tồn tại trong hệ thống"
            };
        }

        // Kiểm tra email đã được verify chưa
        if (user.EmailVerified)
        {
            _logger.LogWarning("Email đã được verify trước đó: {Email}", email);
            return new VerifyEmailResponse
            {
                Success = false,
                Message = "Email đã được xác nhận trước đó"
            };
        }

        // Verify OTP
        var isOtpValid = await _emailVerificationService.VerifyOtpAsync(normalizedEmail, otpCode);
        if (!isOtpValid)
        {
            _logger.LogWarning("OTP không hợp lệ cho email: {Email}", email);
            return new VerifyEmailResponse
            {
                Success = false,
                Message = "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại."
            };
        }

        // Cập nhật trạng thái user (user được lấy từ repository nên cần attach lại để update)
        user.EmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.Status = "ACTIVE"; // Kích hoạt tài khoản
        
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Verify email thành công cho UserId: {UserId}", user.Id);

        // Gửi email chào mừng
        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi welcome email cho UserId: {UserId}", user.Id);
            // Không throw exception, chỉ log lỗi
        }

        return new VerifyEmailResponse
        {
            Success = true,
            Message = "Xác nhận email thành công. Tài khoản của bạn đã được kích hoạt.",
            UserId = user.Id
        };
    }

    /// <summary>
    /// Resend OTP để verify email
    /// </summary>
    public async Task ResendOtpAsync(string email)
    {
        _logger.LogInformation("Resend OTP cho email: {Email}", email);

        var normalizedEmail = email.ToLower().Trim();

        // Tìm user theo email
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
        {
            _logger.LogWarning("Không tìm thấy user với email: {Email}", email);
            throw new InvalidOperationException("Email không tồn tại trong hệ thống");
        }

        // Kiểm tra email đã được verify chưa
        if (user.EmailVerified)
        {
            _logger.LogWarning("Email đã được verify: {Email}", email);
            throw new InvalidOperationException("Email đã được xác nhận trước đó");
        }

        // Resend OTP
        await _emailVerificationService.ResendOtpAsync(normalizedEmail, user.FullName ?? "");
        _logger.LogInformation("Đã resend OTP thành công cho email: {Email}", email);
    }
}

