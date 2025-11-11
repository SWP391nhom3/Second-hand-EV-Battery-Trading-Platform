using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Service xử lý email verification với OTP
/// </summary>
public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailVerificationService> _logger;
    private const int OTP_LENGTH = 6;
    private const int OTP_EXPIRY_MINUTES = 10;
    private const int MAX_ATTEMPTS = 5;

    public EmailVerificationService(
        IEmailVerificationOtpRepository otpRepository,
        IEmailService emailService,
        ILogger<EmailVerificationService> logger)
    {
        _otpRepository = otpRepository;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo và gửi OTP để verify email
    /// </summary>
    public async Task<string> GenerateAndSendOtpAsync(string email, string fullName = "")
    {
        // Tạo OTP code 6 số
        var otpCode = GenerateOtpCode();

        // Tạo OTP entity
        var otp = new EmailVerificationOtp
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Lưu OTP vào database
        await _otpRepository.CreateAsync(otp);
        await _otpRepository.SaveChangesAsync();

        // Gửi email OTP
        try
        {
            await _emailService.SendOtpEmailAsync(email, otpCode, fullName);
            _logger.LogInformation("Đã tạo và gửi OTP thành công cho email: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi OTP email đến: {Email}", email);
            throw new InvalidOperationException("Không thể gửi email OTP. Vui lòng thử lại sau.");
        }

        return otpCode; // Trả về để test, production có thể không cần
    }

    /// <summary>
    /// Verify OTP code
    /// </summary>
    public async Task<bool> VerifyOtpAsync(string email, string otpCode)
    {
        var normalizedEmail = email.ToLower().Trim();

        // Lấy OTP mới nhất chưa sử dụng
        var otp = await _otpRepository.GetLatestValidOtpAsync(normalizedEmail);

        if (otp == null)
        {
            _logger.LogWarning("Không tìm thấy OTP hợp lệ cho email: {Email}", email);
            return false;
        }

        // Kiểm tra số lần thử
        if (otp.AttemptCount >= MAX_ATTEMPTS)
        {
            _logger.LogWarning("OTP đã vượt quá số lần thử cho phép. Email: {Email}, OTPId: {OtpId}", 
                email, otp.Id);
            await _otpRepository.MarkAsUsedAsync(otp.Id);
            await _otpRepository.SaveChangesAsync();
            return false;
        }

        // Tăng số lần thử
        await _otpRepository.IncrementAttemptCountAsync(otp.Id);
        await _otpRepository.SaveChangesAsync();

        // Kiểm tra OTP code
        if (otp.OtpCode != otpCode)
        {
            _logger.LogWarning("OTP code không đúng. Email: {Email}, OTPId: {OtpId}", 
                email, otp.Id);
            return false;
        }

        // Đánh dấu OTP đã sử dụng
        await _otpRepository.MarkAsUsedAsync(otp.Id);
        await _otpRepository.SaveChangesAsync();

        _logger.LogInformation("Verify OTP thành công cho email: {Email}", email);
        return true;
    }

    /// <summary>
    /// Resend OTP cho email
    /// </summary>
    public async Task<string> ResendOtpAsync(string email, string fullName = "")
    {
        _logger.LogInformation("Resend OTP cho email: {Email}", email);
        return await GenerateAndSendOtpAsync(email, fullName);
    }

    /// <summary>
    /// Tạo OTP code ngẫu nhiên 6 số
    /// </summary>
    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

