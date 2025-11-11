using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

/// <summary>
/// Entity lưu trữ OTP để verify email
/// </summary>
public class EmailVerificationOtp : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public int AttemptCount { get; set; } = 0;
}

