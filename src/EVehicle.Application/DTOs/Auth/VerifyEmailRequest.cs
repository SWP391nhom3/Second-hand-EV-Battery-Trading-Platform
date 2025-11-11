namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu verify email với OTP
/// </summary>
public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

