namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Email Service
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email OTP để verify email
    /// </summary>
    Task SendOtpEmailAsync(string email, string otpCode, string fullName = "");

    /// <summary>
    /// Gửi email xác nhận đăng ký thành công
    /// </summary>
    Task SendWelcomeEmailAsync(string email, string fullName = "");
}

