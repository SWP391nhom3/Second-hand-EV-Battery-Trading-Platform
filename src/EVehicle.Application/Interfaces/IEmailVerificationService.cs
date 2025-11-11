namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Email Verification Service
/// </summary>
public interface IEmailVerificationService
{
    /// <summary>
    /// Tạo và gửi OTP để verify email
    /// </summary>
    Task<string> GenerateAndSendOtpAsync(string email, string fullName = "");

    /// <summary>
    /// Verify OTP code
    /// </summary>
    Task<bool> VerifyOtpAsync(string email, string otpCode);

    /// <summary>
    /// Resend OTP cho email
    /// </summary>
    Task<string> ResendOtpAsync(string email, string fullName = "");
}

