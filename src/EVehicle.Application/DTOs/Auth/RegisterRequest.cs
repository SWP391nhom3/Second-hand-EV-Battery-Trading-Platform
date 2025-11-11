namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu đăng ký tài khoản (UC01)
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

