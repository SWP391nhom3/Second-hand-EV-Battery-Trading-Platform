using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Email Verification OTP Repository
/// </summary>
public interface IEmailVerificationOtpRepository
{
    /// <summary>
    /// Tạo OTP mới cho email
    /// </summary>
    Task<EmailVerificationOtp> CreateAsync(EmailVerificationOtp otp);

    /// <summary>
    /// Lấy OTP mới nhất chưa sử dụng cho email
    /// </summary>
    Task<EmailVerificationOtp?> GetLatestValidOtpAsync(string email);

    /// <summary>
    /// Đánh dấu OTP đã được sử dụng
    /// </summary>
    Task MarkAsUsedAsync(Guid otpId);

    /// <summary>
    /// Tăng số lần thử của OTP
    /// </summary>
    Task IncrementAttemptCountAsync(Guid otpId);

    /// <summary>
    /// Xóa các OTP đã hết hạn
    /// </summary>
    Task DeleteExpiredOtpsAsync();

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync();
}

