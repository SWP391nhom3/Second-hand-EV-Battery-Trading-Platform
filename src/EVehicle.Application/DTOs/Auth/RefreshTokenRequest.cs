namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho request refresh token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token (được nhận từ login/register)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

