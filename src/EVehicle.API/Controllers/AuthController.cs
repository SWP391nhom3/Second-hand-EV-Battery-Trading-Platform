using EVehicle.Application.DTOs.Auth;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API Authentication (UC01, UC02, UC03)
/// </summary>
/// <remarks>
/// API này cung cấp các chức năng xác thực người dùng bao gồm:
/// - UC01: Đăng ký tài khoản mới
/// - UC02: Đăng nhập bằng Email/Số điện thoại và Password
/// - UC03: Đăng nhập bằng mạng xã hội (Google, Facebook)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<SocialLoginRequest> _socialLoginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    private readonly IValidator<ResendOtpRequest> _resendOtpValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<SocialLoginRequest> socialLoginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator,
        IValidator<ResendOtpRequest> resendOtpValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _socialLoginValidator = socialLoginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _resendOtpValidator = resendOtpValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC01: Đăng ký tài khoản mới
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "email": "user@example.com",
    ///         "phoneNumber": "0987654321",
    ///         "password": "StrongPass123!",
    ///         "confirmPassword": "StrongPass123!",
    ///         "fullName": "Nguyễn Văn A"
    ///     }
    /// 
    /// Validation:
    /// - Email: Bắt buộc, định dạng email hợp lệ
    /// - PhoneNumber: Bắt buộc, định dạng 0xxxxxxxxx (10 số)
    /// - Password: Tối thiểu 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt
    /// - ConfirmPassword: Phải khớp với Password
    /// - FullName: Tùy chọn, tối đa 100 ký tự
    /// </remarks>
    /// <param name="request">Thông tin đăng ký tài khoản</param>
    /// <returns>Thông tin user và JWT token</returns>
    /// <response code="201">Đăng ký thành công, trả về thông tin user và token</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc Email/Phone đã tồn tại</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var response = await _authService.RegisterAsync(request);

            _logger.LogInformation("Đăng ký tài khoản thành công, UserId: {UserId}", response.UserId);

            return CreatedAtAction(
                nameof(GetCurrentUser),
                new { },
                response
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi đăng ký: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng ký tài khoản");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng ký tài khoản" });
        }
    }

    /// <summary>
    /// UC02: Đăng nhập bằng Email/Số điện thoại và Password
    /// </summary>
    /// <remarks>
    /// Sample request với Email:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "emailOrPhone": "user@example.com",
    ///         "password": "StrongPass123!"
    ///     }
    /// 
    /// Sample request với Phone:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "emailOrPhone": "0987654321",
    ///         "password": "StrongPass123!"
    ///     }
    /// 
    /// Hỗ trợ đăng nhập bằng cả Email lẫn Số điện thoại.
    /// </remarks>
    /// <param name="request">Thông tin đăng nhập (Email hoặc Phone và Password)</param>
    /// <returns>Thông tin user và JWT token</returns>
    /// <response code="200">Đăng nhập thành công, trả về thông tin user và token</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="401">Email/Phone hoặc mật khẩu không đúng, hoặc tài khoản bị khóa</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var response = await _authService.LoginAsync(request);

            _logger.LogInformation("Đăng nhập thành công, UserId: {UserId}", response.UserId);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Lỗi đăng nhập: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng nhập" });
        }
    }

    /// <summary>
    /// UC03: Đăng nhập bằng mạng xã hội (Google, Facebook)
    /// </summary>
    /// <remarks>
    /// Sample request với Google:
    /// 
    ///     POST /api/auth/social-login
    ///     {
    ///         "provider": "google",
    ///         "token": "google-id-token-here"
    ///     }
    /// 
    /// Sample request với Facebook:
    /// 
    ///     POST /api/auth/social-login
    ///     {
    ///         "provider": "facebook",
    ///         "token": "facebook-access-token-here"
    ///     }
    /// 
    /// Luồng xử lý:
    /// - Xác thực token với provider (Google/Facebook)
    /// - Nếu email đã tồn tại: Liên kết với tài khoản hiện có
    /// - Nếu email chưa tồn tại: Tạo tài khoản mới
    /// - Trả về JWT token
    /// 
    /// Lưu ý:
    /// - Google: Sử dụng ID Token (không phải Access Token)
    /// - Facebook: Sử dụng Access Token
    /// </remarks>
    /// <param name="request">Thông tin đăng nhập social (provider và token)</param>
    /// <returns>Thông tin user và JWT token</returns>
    /// <response code="200">Đăng nhập thành công, trả về thông tin user và token</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc provider không được hỗ trợ</response>
    /// <response code="401">Token không hợp lệ hoặc tài khoản bị khóa</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("social-login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _socialLoginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var response = await _authService.SocialLoginAsync(request);

            _logger.LogInformation("Đăng nhập social thành công, UserId: {UserId}, Provider: {Provider}", 
                response.UserId, request.Provider);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Lỗi đăng nhập social: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Provider không hợp lệ: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập social");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đăng nhập bằng mạng xã hội" });
        }
    }

    /// <summary>
    /// Refresh Access Token và Refresh Token
    /// </summary>
    /// <remarks>
    /// Endpoint này cho phép refresh Access Token và Refresh Token khi Access Token đã hết hạn.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/auth/refresh-token
    ///     {
    ///         "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    ///     }
    /// 
    /// Luồng xử lý:
    /// - Validate refresh token (kiểm tra signature, expiry, và tokenType = "refresh")
    /// - Lấy thông tin user từ database
    /// - Kiểm tra tài khoản còn ACTIVE
    /// - Tạo Access Token mới (thời gian ngắn, ví dụ: 60 phút)
    /// - Tạo Refresh Token mới (thời gian dài, ví dụ: 30 ngày)
    /// - Trả về cả Access Token và Refresh Token mới
    /// 
    /// Lưu ý:
    /// - Refresh Token có thời gian hết hạn dài hơn Access Token (ví dụ: 30 ngày vs 60 phút)
    /// - Sau khi refresh, Refresh Token cũ sẽ không còn hiệu lực
    /// - Client nên lưu cả Access Token và Refresh Token
    /// - Khi Access Token hết hạn, sử dụng Refresh Token để lấy token mới
    /// </remarks>
    /// <param name="request">Thông tin refresh token</param>
    /// <returns>Thông tin user, Access Token mới và Refresh Token mới</returns>
    /// <response code="200">Refresh token thành công, trả về Access Token và Refresh Token mới</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="401">Refresh Token không hợp lệ, đã hết hạn, hoặc tài khoản bị khóa</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _refreshTokenValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var response = await _authService.RefreshTokenAsync(request.RefreshToken);

            _logger.LogInformation("Refresh token thành công, UserId: {UserId}", response.UserId);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Lỗi refresh token: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi refresh token");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi refresh token" });
        }
    }

    /// <summary>
    /// Lấy thông tin user hiện tại từ JWT token
    /// </summary>
    /// <remarks>
    /// Endpoint này yêu cầu authentication. Sử dụng JWT token nhận được từ đăng ký/đăng nhập.
    /// 
    /// Thêm token vào header:
    /// 
    ///     Authorization: Bearer {your-jwt-token}
    /// 
    /// Trong Swagger UI, click nút "Authorize" và nhập: Bearer {token}
    /// </remarks>
    /// <returns>Thông tin chi tiết của user hiện tại</returns>
    /// <response code="200">Trả về thông tin user thành công</response>
    /// <response code="401">Token không hợp lệ, đã hết hạn, hoặc không có token</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var accessToken = authHeader.Substring("Bearer ".Length).Trim();
            var user = await _authService.ValidateTokenAsync(accessToken);

            if (user == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin user");
            return StatusCode(500, new { message = "Đã xảy ra lỗi" });
        }
    }

    /// <summary>
    /// Verify email với OTP code
    /// </summary>
    /// <remarks>
    /// Endpoint này dùng để xác nhận email sau khi đăng ký tài khoản.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/auth/verify-email
    ///     {
    ///         "email": "user@example.com",
    ///         "otpCode": "123456"
    ///     }
    /// 
    /// Sau khi verify thành công, tài khoản sẽ được kích hoạt và user có thể đăng nhập.
    /// </remarks>
    /// <param name="request">Thông tin email và OTP code</param>
    /// <returns>Kết quả verify email</returns>
    /// <response code="200">Verify email thành công hoặc thất bại (kiểm tra Success field)</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _verifyEmailValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var response = await _authService.VerifyEmailAsync(request.Email, request.OtpCode);

            if (response.Success)
            {
                _logger.LogInformation("Verify email thành công cho email: {Email}", request.Email);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Verify email thất bại cho email: {Email}, Message: {Message}", 
                    request.Email, response.Message);
                return Ok(response); // Vẫn trả về 200 nhưng Success = false
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi verify email");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi verify email" });
        }
    }

    /// <summary>
    /// Resend OTP để verify email
    /// </summary>
    /// <remarks>
    /// Endpoint này dùng để gửi lại OTP code nếu user chưa nhận được hoặc đã hết hạn.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/auth/resend-otp
    ///     {
    ///         "email": "user@example.com"
    ///     }
    /// </remarks>
    /// <param name="request">Thông tin email</param>
    /// <returns>Kết quả resend OTP</returns>
    /// <response code="200">Resend OTP thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc email không tồn tại/đã verify</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("resend-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        try
        {
            // Validate request
            var validationResult = await _resendOtpValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            await _authService.ResendOtpAsync(request.Email);

            _logger.LogInformation("Resend OTP thành công cho email: {Email}", request.Email);

            return Ok(new
            {
                message = "Đã gửi lại mã OTP. Vui lòng kiểm tra email."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi resend OTP: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi resend OTP");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi gửi lại OTP" });
        }
    }

    /// <summary>
    /// Health check endpoint cho Auth service
    /// </summary>
    /// <remarks>
    /// Endpoint kiểm tra trạng thái hoạt động của Auth service.
    /// Không yêu cầu authentication.
    /// </remarks>
    /// <returns>Trạng thái của service</returns>
    /// <response code="200">Service đang hoạt động bình thường</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "AuthController",
            timestamp = DateTime.UtcNow
        });
    }
}

