using EVehicle.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// Service xử lý xác thực token từ Google và Facebook
/// </summary>
public class SocialAuthService : ISocialAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SocialAuthService> _logger;
    private readonly HttpClient _httpClient;

    public SocialAuthService(
        IConfiguration configuration,
        ILogger<SocialAuthService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Xác thực token và lấy thông tin user từ provider
    /// </summary>
    public async Task<(string Email, string? FullName, string SocialLoginId)> ValidateAndGetUserInfoAsync(
        string provider, 
        string token)
    {
        return provider.ToLower() switch
        {
            "google" => await ValidateGoogleTokenAsync(token),
            "facebook" => await ValidateFacebookTokenAsync(token),
            _ => throw new ArgumentException($"Provider không được hỗ trợ: {provider}")
        };
    }

    /// <summary>
    /// Xác thực Google ID Token
    /// </summary>
    private async Task<(string Email, string? FullName, string SocialLoginId)> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var googleClientId = _configuration["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                throw new InvalidOperationException("Google ClientId chưa được cấu hình");
            }

            // Xác thực Google ID Token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            });

            _logger.LogInformation("Xác thực Google token thành công, Email: {Email}", payload.Email);

            return (
                Email: payload.Email,
                FullName: payload.Name,
                SocialLoginId: payload.Subject
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xác thực Google token");
            throw new UnauthorizedAccessException("Google token không hợp lệ");
        }
    }

    /// <summary>
    /// Xác thực Facebook Access Token
    /// </summary>
    private async Task<(string Email, string? FullName, string SocialLoginId)> ValidateFacebookTokenAsync(string accessToken)
    {
        try
        {
            var facebookAppId = _configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];

            if (string.IsNullOrEmpty(facebookAppId) || string.IsNullOrEmpty(facebookAppSecret))
            {
                throw new InvalidOperationException("Facebook AppId/AppSecret chưa được cấu hình");
            }

            // Debug token với Facebook Graph API
            var debugUrl = $"https://graph.facebook.com/debug_token?" +
                          $"input_token={accessToken}&" +
                          $"access_token={facebookAppId}|{facebookAppSecret}";

            var debugResponse = await _httpClient.GetFromJsonAsync<FacebookDebugTokenResponse>(debugUrl);

            if (debugResponse?.Data == null || !debugResponse.Data.IsValid)
            {
                throw new UnauthorizedAccessException("Facebook token không hợp lệ");
            }

            // Lấy thông tin user từ Facebook
            var userUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";
            var userResponse = await _httpClient.GetFromJsonAsync<FacebookUserResponse>(userUrl);

            if (userResponse == null || string.IsNullOrEmpty(userResponse.Email))
            {
                throw new UnauthorizedAccessException("Không thể lấy thông tin user từ Facebook");
            }

            _logger.LogInformation("Xác thực Facebook token thành công, Email: {Email}", userResponse.Email);

            return (
                Email: userResponse.Email,
                FullName: userResponse.Name,
                SocialLoginId: userResponse.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xác thực Facebook token");
            throw new UnauthorizedAccessException("Facebook token không hợp lệ");
        }
    }

    #region Facebook Response Models

    private class FacebookDebugTokenResponse
    {
        public FacebookDebugTokenData? Data { get; set; }
    }

    private class FacebookDebugTokenData
    {
        public bool IsValid { get; set; }
        public string? AppId { get; set; }
        public string? UserId { get; set; }
    }

    private class FacebookUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    #endregion
}

