using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using EVehicleManagementAPI.Options;

namespace EVehicleManagementAPI.Services
{
    public class PayOsService
    {
        private readonly HttpClient _httpClient;
        private readonly PayOsOptions _options;

        public PayOsService(HttpClient httpClient, IOptions<PayOsOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
            if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            }

            // Simple header scheme - adjust if PayOS requires different auth headers
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            }
        }

        public string GenerateTransferContent(int postId, string packageCode, int randomLen = 6)
        {
            var rand = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("/", string.Empty);
            var suffix = rand.Substring(0, Math.Min(randomLen, rand.Length)).ToUpper();
            return $"POST-{postId}-{packageCode}-{suffix}";
        }

        public static string ComputeHmacSha256(string secret, string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        public bool VerifyWebhookSignature(string checksumKey, string rawBody, string? signature)
        {
            if (string.IsNullOrEmpty(signature)) return false;
            var computed = ComputeHmacSha256(checksumKey, rawBody);
            return string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<(string checkoutUrl, string orderCode)> CreateOrderAsync(
            long amount,
            string description,
            string orderCode,
            string? returnUrl,
            string? cancelUrl,
            CancellationToken ct = default)
        {
            // Adjust path/payload according to PayOS API spec
            var payload = new
            {
                amount,
                description,
                orderCode,
                returnUrl = returnUrl ?? _options.ReturnUrl,
                cancelUrl = cancelUrl ?? _options.CancelUrl,
                // clientId may be required by PayOS
                clientId = _options.ClientId
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/orders", payload, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            var checkoutUrl = root.GetProperty("data").GetProperty("checkoutUrl").GetString() ?? string.Empty;
            var orderCodeResp = root.GetProperty("data").GetProperty("orderCode").GetString() ?? orderCode;
            return (checkoutUrl, orderCodeResp);
        }
    }
}


