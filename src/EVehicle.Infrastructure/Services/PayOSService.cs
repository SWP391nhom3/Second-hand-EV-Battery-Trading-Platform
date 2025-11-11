using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// PayOS Service implementation
/// Tích hợp với PayOS API để tạo payment link và QR code
/// </summary>
public class PayOSService : IPayOSService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayOSService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;
    private readonly string _apiUrl;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;

    public PayOSService(
        IConfiguration configuration,
        ILogger<PayOSService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        // Get PayOS configuration
        _clientId = _configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS:ClientId chưa được cấu hình");
        _apiKey = _configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS:ApiKey chưa được cấu hình");
        _checksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS:ChecksumKey chưa được cấu hình");
        _apiUrl = _configuration["PayOS:ApiUrl"] ?? "https://api-merchant.payos.vn";
        
        // Default return URLs (will be overridden by parameters in CreatePaymentLinkAsync)
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        _returnUrl = $"{frontendUrl}/payment/success";
        _cancelUrl = $"{frontendUrl}/payment/cancel";

        // Configure HttpClient base address
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_apiUrl);
        }
    }

    /// <summary>
    /// Tạo payment link từ PayOS
    /// </summary>
    public async Task<BaseResponse<PayOSPaymentLinkResponse>> CreatePaymentLinkAsync(
        long orderCode,
        decimal amount,
        string description,
        string? returnUrl = null,
        string? cancelUrl = null)
    {
        try
        {
            // Convert amount to integer (VND)
            var amountInVnd = (long)amount;
            var finalReturnUrl = returnUrl ?? _returnUrl;
            var finalCancelUrl = cancelUrl ?? _cancelUrl;

            // Calculate checksum (signature) - PayOS requires signature from specific fields
            var signature = CalculateChecksum(orderCode, amountInVnd, description, finalReturnUrl, finalCancelUrl);

            // Prepare request data with signature
            var requestData = new
            {
                orderCode = orderCode,
                amount = amountInVnd,
                description = description,
                returnUrl = finalReturnUrl,
                cancelUrl = finalCancelUrl,
                items = new[]
                {
                    new
                    {
                        name = description,
                        quantity = 1,
                        price = amountInVnd
                    }
                },
                signature = signature
            };

            // Prepare HTTP request
            // PayOS API yêu cầu JSON với property naming là camelCase
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            var json = JsonSerializer.Serialize(requestData, jsonOptions);
            
            // Log request body để debug
            _logger.LogInformation("PayOS request body: {RequestBody}", json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Create HTTP request with headers
            var request = new HttpRequestMessage(HttpMethod.Post, "/v2/payment-requests")
            {
                Content = content
            };
            
            // Add PayOS required headers
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            _logger.LogInformation("Calling PayOS API to create payment link. OrderCode: {OrderCode}, Amount: {Amount}, Signature: {Signature}", 
                orderCode, amountInVnd, signature);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("PayOS API response: {StatusCode}, Content: {Content}", response.StatusCode, responseContent);

            // Parse response - PayOS API returns data directly or in data field
            var responseJsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayOS API error: {StatusCode}, Response: {Response}", response.StatusCode, responseContent);
                
                // Parse error response from PayOS if available
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<PayOSApiResponse>(responseContent, responseJsonOptions);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Desc))
                    {
                        return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                            $"Lỗi từ PayOS API: {errorResponse.Desc} (Code: {errorResponse.Code})");
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
                
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    $"Lỗi khi tạo payment link từ PayOS. Status: {response.StatusCode}, Response: {responseContent}");
            }
            
            var payOSResponse = JsonSerializer.Deserialize<PayOSApiResponse>(responseContent, responseJsonOptions);

            // Check if response has error code
            if (payOSResponse != null && !string.IsNullOrEmpty(payOSResponse.Code) && payOSResponse.Code != "00" && payOSResponse.Code != "0")
            {
                _logger.LogError("PayOS API returned error: Code={Code}, Desc={Desc}, Response={Response}", 
                    payOSResponse.Code, payOSResponse.Desc, responseContent);
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    $"Lỗi từ PayOS: {payOSResponse.Desc ?? "Unknown error"}");
            }

            // Try to extract checkoutUrl and qrCode from response
            string? checkoutUrl = null;
            string? qrCode = null;

            if (payOSResponse?.Data != null)
            {
                checkoutUrl = payOSResponse.Data.CheckoutUrl;
                qrCode = payOSResponse.Data.QrCode;
            }
            else
            {
                // Try to parse directly from response (some PayOS API versions return data directly)
                var directResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, responseJsonOptions);
                if (directResponse != null)
                {
                    if (directResponse.TryGetValue("checkoutUrl", out var checkoutUrlObj))
                    {
                        checkoutUrl = checkoutUrlObj?.ToString();
                    }
                    if (directResponse.TryGetValue("qrCode", out var qrCodeObj))
                    {
                        qrCode = qrCodeObj?.ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(checkoutUrl))
            {
                _logger.LogError("PayOS API response does not contain checkoutUrl. Response: {Response}", responseContent);
                return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                    "Không thể lấy payment URL từ PayOS. Vui lòng thử lại sau.");
            }

            // Return response
            var result = new PayOSPaymentLinkResponse
            {
                PaymentUrl = checkoutUrl,
                QrCodeUrl = qrCode ?? string.Empty,
                OrderCode = orderCode,
                Amount = amount
            };

            _logger.LogInformation("PayOS payment link created successfully. OrderCode: {OrderCode}, PaymentUrl: {PaymentUrl}", orderCode, result.PaymentUrl);

            return BaseResponse<PayOSPaymentLinkResponse>.SuccessResponse(
                result,
                "Tạo payment link thành công");
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            // Handle network/DNS errors
            _logger.LogError(httpEx, "Lỗi kết nối đến PayOS API. OrderCode: {OrderCode}, ApiUrl: {ApiUrl}", orderCode, _apiUrl);
            
            if (httpEx.InnerException is System.Net.Sockets.SocketException socketEx)
            {
                if (socketEx.Message.Contains("nodename nor servname provided") || 
                    socketEx.Message.Contains("Could not resolve host") ||
                    socketEx.Message.Contains("Name or service not known"))
                {
                    return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                        $"Không thể kết nối đến PayOS API ({_apiUrl}). " +
                        $"Lỗi DNS: Không thể resolve domain name. " +
                        $"Vui lòng kiểm tra kết nối mạng và cấu hình DNS. " +
                        $"Chi tiết: {socketEx.Message}");
                }
            }
            
            return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                $"Lỗi kết nối đến PayOS API: {httpEx.Message}. " +
                $"Vui lòng kiểm tra kết nối mạng và cấu hình PayOS API URL.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo payment link từ PayOS. OrderCode: {OrderCode}", orderCode);
            return BaseResponse<PayOSPaymentLinkResponse>.FailureResponse(
                ex,
                $"Đã xảy ra lỗi khi tạo payment link từ PayOS: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculate signature using HMAC SHA256
    /// PayOS signature format: amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
    /// Data được sort theo alphabet và format như query string
    /// Reference: https://payos.vn/docs/api/#tag/payment-request/operation/payment-request
    /// Reference: https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/
    /// 
    /// QUAN TRỌNG: Theo tài liệu PayOS và ví dụ PHP (http_build_query):
    /// - Signature được tính từ 5 trường: amount, cancelUrl, description, orderCode, returnUrl
    /// - Các giá trị cần được URL encode bằng encodeURIComponent (Uri.EscapeDataString)
    /// - http_build_query() trong PHP tự động URL encode TẤT CẢ các giá trị
    /// - Sắp xếp theo thứ tự alphabet của tên trường
    /// </summary>
    private string CalculateChecksum(long orderCode, long amount, string description, string returnUrl, string cancelUrl)
    {
        // PayOS signature calculation theo tài liệu:
        // 1. Sắp xếp các trường theo alphabet: amount, cancelUrl, description, orderCode, returnUrl
        // 2. URL encode TẤT CẢ các giá trị bằng Uri.EscapeDataString (encodeURIComponent)
        //    - PHP http_build_query() encode TẤT CẢ các giá trị, bao gồm cả URL
        // 3. Format: amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
        // 4. Tính HMAC SHA256 với checksumKey
        
        // URL encode TẤT CẢ các giá trị (bao gồm cả URL)
        // Uri.EscapeDataString tương đương với encodeURIComponent trong JavaScript
        // PHP http_build_query() tự động encode tất cả giá trị
        var encodedCancelUrl = Uri.EscapeDataString(cancelUrl);
        var encodedDescription = Uri.EscapeDataString(description);
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
        
        // Build query string với các trường đã sort alphabetically
        // Format: amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
        var dataToSign = $"amount={amount}&cancelUrl={encodedCancelUrl}&description={encodedDescription}&orderCode={orderCode}&returnUrl={encodedReturnUrl}";
        
        // Log chi tiết để debug
        _logger.LogInformation("=== PayOS Signature Calculation ===");
        _logger.LogInformation("Input parameters:");
        _logger.LogInformation("  OrderCode: {OrderCode}", orderCode);
        _logger.LogInformation("  Amount: {Amount}", amount);
        _logger.LogInformation("  Description (raw): '{Description}'", description);
        _logger.LogInformation("  Description (encoded): '{EncodedDescription}'", encodedDescription);
        _logger.LogInformation("  ReturnUrl (raw): '{ReturnUrl}'", returnUrl);
        _logger.LogInformation("  ReturnUrl (encoded): '{EncodedReturnUrl}'", encodedReturnUrl);
        _logger.LogInformation("  CancelUrl (raw): '{CancelUrl}'", cancelUrl);
        _logger.LogInformation("  CancelUrl (encoded): '{EncodedCancelUrl}'", encodedCancelUrl);
        _logger.LogInformation("Data to sign (ALL values URL encoded): '{DataToSign}'", dataToSign);
        _logger.LogInformation("ChecksumKey: {KeyPrefix}... (length: {Length} chars)", 
            _checksumKey.Substring(0, Math.Min(10, _checksumKey.Length)), _checksumKey.Length);
        
        // Calculate HMAC SHA256 với UTF-8 encoding
        byte[] keyBytes = Encoding.UTF8.GetBytes(_checksumKey);
        byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);
        
        using var hmac = new HMACSHA256(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(dataBytes);
        var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        
        _logger.LogInformation("Signature (HMAC SHA256): {Signature}", signature);
        _logger.LogInformation("=== End Signature Calculation ===");
        
        return signature;
    }

    /// <summary>
    /// PayOS API Response model
    /// Reference: https://payos.vn/docs/api/#tag/payment-request/operation/payment-request
    /// </summary>
    private class PayOSApiResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public PayOSApiData? Data { get; set; }
    }

    /// <summary>
    /// PayOS API Data model
    /// Response data contains checkoutUrl and qrCode
    /// Property names are case-insensitive (handled by JsonSerializerOptions)
    /// </summary>
    private class PayOSApiData
    {
        public string? CheckoutUrl { get; set; }
        public string? QrCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public long? Amount { get; set; }
        public string? Description { get; set; }
        public long? OrderCode { get; set; }
        public string? Status { get; set; }
    }
}

