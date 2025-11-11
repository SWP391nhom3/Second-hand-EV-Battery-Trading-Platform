namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Social Authentication Service
/// Xác thực token từ Google, Facebook
/// </summary>
public interface ISocialAuthService
{
    /// <summary>
    /// Xác thực token từ provider và lấy thông tin user
    /// </summary>
    /// <param name="provider">google hoặc facebook</param>
    /// <param name="token">Access token hoặc ID token</param>
    /// <returns>Tuple (Email, FullName, SocialLoginId)</returns>
    Task<(string Email, string? FullName, string SocialLoginId)> ValidateAndGetUserInfoAsync(
        string provider, 
        string token);
}

