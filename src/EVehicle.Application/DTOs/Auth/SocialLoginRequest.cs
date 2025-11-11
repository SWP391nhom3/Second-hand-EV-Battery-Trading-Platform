namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu đăng nhập bằng mạng xã hội (UC03)
/// </summary>
public class SocialLoginRequest
{
    /// <summary>
    /// Provider: 'google', 'facebook'
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token hoặc ID token từ provider
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

