using EVehicle.Application.DTOs.Auth;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Authentication Service
/// Xử lý các nghiệp vụ UC01, UC02, UC03
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// UC01: Đăng ký tài khoản mới
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// UC02: Đăng nhập bằng Email/Phone và Password
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// UC03: Đăng nhập bằng mạng xã hội (Google, Facebook)
    /// </summary>
    Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request);

    /// <summary>
    /// Xác thực JWT token
    /// </summary>
    Task<UserDto?> ValidateTokenAsync(string token);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string token);

    /// <summary>
    /// Verify email với OTP code
    /// </summary>
    Task<VerifyEmailResponse> VerifyEmailAsync(string email, string otpCode);

    /// <summary>
    /// Resend OTP để verify email
    /// </summary>
    Task ResendOtpAsync(string email);
}

