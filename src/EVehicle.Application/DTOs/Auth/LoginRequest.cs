namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu đăng nhập (UC02)
/// Hỗ trợ đăng nhập bằng Email hoặc Số điện thoại
/// </summary>
public class LoginRequest
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

