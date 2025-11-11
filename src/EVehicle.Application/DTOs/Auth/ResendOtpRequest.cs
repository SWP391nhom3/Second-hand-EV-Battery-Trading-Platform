namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu resend OTP
/// </summary>
public class ResendOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

