using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EVehicleManagementAPI.Services
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public GoogleOAuthService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _http = httpClientFactory.CreateClient();
        }

        public string BuildAuthorizationUrl(string state)
        {
            var clientId = _config["Authentication:Google:ClientId"];
            var redirectUri = _config["Authentication:Google:RedirectUri"];
            var scope = Uri.EscapeDataString("openid email profile");
            var url = $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={scope}&state={Uri.EscapeDataString(state)}&prompt=select_account";
            return url;
        }

        public async Task<GoogleUserInfo> ExchangeCodeForUserAsync(string code, string redirectUri)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var clientId = _config["Authentication:Google:ClientId"];
            var clientSecret = _config["Authentication:Google:ClientSecret"];

            var form = new StringContent($"code={Uri.EscapeDataString(code)}&client_id={Uri.EscapeDataString(clientId)}&client_secret={Uri.EscapeDataString(clientSecret)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&grant_type=authorization_code", Encoding.UTF8, "application/x-www-form-urlencoded");
            var tokenResp = await _http.PostAsync(tokenEndpoint, form);
            if (!tokenResp.IsSuccessStatusCode)
            {
                var errorText = await tokenResp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Google token exchange failed: {(int)tokenResp.StatusCode} {tokenResp.ReasonPhrase}. Body: {errorText}");
            }
            var tokenJson = await tokenResp.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
            var idToken = tokenDoc.RootElement.TryGetProperty("id_token", out var idTokEl) ? idTokEl.GetString() : null;

            // Prefer userinfo endpoint
            var req = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var userResp = await _http.SendAsync(req);
            userResp.EnsureSuccessStatusCode();
            var userJson = await userResp.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<GoogleUserInfo>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return user;
        }
    }
}


