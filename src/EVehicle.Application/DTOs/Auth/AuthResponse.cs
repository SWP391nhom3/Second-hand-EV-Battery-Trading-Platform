namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho response sau khi đăng ký/đăng nhập thành công
/// </summary>
public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? AccessTokenExpiry { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? Message { get; set; }
}

