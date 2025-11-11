using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// AI Price Service implementation sử dụng Google Gemini API (UC51)
/// </summary>
public class AIPriceService : IAIPriceService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIPriceService> _logger;
    private readonly HttpClient _httpClient;

    public AIPriceService(
        IConfiguration configuration,
        ILogger<AIPriceService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<BaseResponse<PriceSuggestionResponse>> SuggestPriceAsync(PriceSuggestionRequest request)
    {
        try
        {
            var apiKey = _configuration["GoogleGemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Google Gemini API Key chưa được cấu hình");
                return BaseResponse<PriceSuggestionResponse>.FailureResponse(
                    "Dịch vụ AI gợi ý giá tạm thời không khả dụng. Vui lòng thử lại sau.");
            }

            // Tạo prompt cho Gemini
            var prompt = BuildPrompt(request);

            // Gọi Google Gemini API
            var geminiResponse = await CallGeminiAPIAsync(apiKey, prompt);

            if (geminiResponse == null)
            {
                return BaseResponse<PriceSuggestionResponse>.FailureResponse(
                    "Không thể kết nối đến dịch vụ AI. Vui lòng thử lại sau.");
            }

            // Parse response từ Gemini
            var suggestion = ParseGeminiResponse(geminiResponse, request);

            var response = new PriceSuggestionResponse
            {
                SuggestedPrice = suggestion.SuggestedPrice,
                ConfidenceScore = suggestion.ConfidenceScore,
                Factors = suggestion.Factors,
                Analysis = suggestion.Analysis
            };

            _logger.LogInformation(
                "AI gợi ý giá thành công: {SuggestedPrice} VND, Confidence: {ConfidenceScore}%",
                suggestion.SuggestedPrice,
                suggestion.ConfidenceScore);

            return BaseResponse<PriceSuggestionResponse>.SuccessResponse(
                response,
                "Gợi ý giá đã được tạo thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi AI service để gợi ý giá");
            return BaseResponse<PriceSuggestionResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo gợi ý giá. Vui lòng thử lại sau.");
        }
    }

    /// <summary>
    /// Xây dựng prompt cho Gemini dựa trên thông tin sản phẩm
    /// </summary>
    private string BuildPrompt(PriceSuggestionRequest request)
    {
        var category = request.CategoryId == 1 ? "Xe điện" : "Pin";
        var mileageInfo = request.Mileage.HasValue 
            ? $"\n- Số KM đã đi: {request.Mileage:N0} km" 
            : "";
        var chargeCountInfo = request.ChargeCount.HasValue 
            ? $"\n- Số lần sạc: {request.ChargeCount:N0} lần" 
            : "";
        var descriptionInfo = !string.IsNullOrEmpty(request.Description)
            ? $"\n- Mô tả: {request.Description}"
            : "";

        return $@"Bạn là chuyên gia định giá xe điện và pin đã qua sử dụng tại Việt Nam. 
Hãy phân tích và đưa ra giá gợi ý cho sản phẩm sau:

**Thông tin sản phẩm:**
- Loại: {category}
- Thương hiệu: {request.Brand}
- Model: {request.Model}
- Năm sản xuất: {request.ProductionYear}
- Dung lượng pin hiện tại: {request.BatteryCapacityCurrent} kWh/Ah
{chargeCountInfo}{mileageInfo}
- Tình trạng: {request.Condition}
- Địa điểm: {request.Location}{descriptionInfo}

**Yêu cầu:**
1. Phân tích giá thị trường dựa trên các yếu tố: thương hiệu, model, năm sản xuất, tình trạng pin (SOH), số km (nếu là xe), vị trí địa lý, và xu hướng thị trường hiện tại tại Việt Nam.
2. Đưa ra giá gợi ý phù hợp (đơn vị: VNĐ).
3. Đánh giá độ tin cậy của gợi ý (0-100%).
4. Liệt kê các yếu tố ảnh hưởng đến giá.

**Định dạng trả về (JSON):**
{{
  ""suggestedPrice"": <số tiền VNĐ>,
  ""confidenceScore"": <0-100>,
  ""factors"": {{
    ""brand"": ""Ảnh hưởng của thương hiệu"",
    ""model"": ""Ảnh hưởng của model"",
    ""year"": ""Ảnh hưởng của năm sản xuất"",
    ""battery"": ""Ảnh hưởng của tình trạng pin"",
    ""condition"": ""Ảnh hưởng của tình trạng"",
    ""location"": ""Ảnh hưởng của vị trí"",
    ""marketTrend"": ""Xu hướng thị trường""
  }},
  ""analysis"": ""Phân tích chi tiết về giá gợi ý""
}}

Chỉ trả về JSON, không có text thêm.";
    }

    /// <summary>
    /// Gọi Google Gemini API
    /// </summary>
    private async Task<string?> CallGeminiAPIAsync(string apiKey, string prompt)
    {
        try
        {
            var model = _configuration["GoogleGemini:Model"] ?? "gemini-pro";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            if (geminiResponse?.Candidates != null && 
                geminiResponse.Candidates.Length > 0 &&
                geminiResponse.Candidates[0].Content?.Parts != null &&
                geminiResponse.Candidates[0].Content.Parts.Length > 0)
            {
                return geminiResponse.Candidates[0].Content.Parts[0].Text;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Google Gemini API");
            return null;
        }
    }

    /// <summary>
    /// Parse response từ Gemini thành PriceSuggestionResponse
    /// </summary>
    private (decimal SuggestedPrice, decimal ConfidenceScore, string? Factors, string? Analysis) 
        ParseGeminiResponse(string geminiResponse, PriceSuggestionRequest request)
    {
        try
        {
            // Tìm JSON trong response (có thể có text thêm)
            var jsonStart = geminiResponse.IndexOf('{');
            var jsonEnd = geminiResponse.LastIndexOf('}') + 1;
            
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                _logger.LogWarning("Không tìm thấy JSON trong response từ Gemini");
                return GetFallbackSuggestion(request);
            }

            var jsonText = geminiResponse.Substring(jsonStart, jsonEnd - jsonStart);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<GeminiPriceSuggestion>(jsonText, options);

            if (parsed == null)
            {
                return GetFallbackSuggestion(request);
            }

            var factorsJson = parsed.Factors != null 
                ? JsonSerializer.Serialize(parsed.Factors) 
                : null;

            return (
                parsed.SuggestedPrice,
                parsed.ConfidenceScore,
                factorsJson,
                parsed.Analysis
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi parse response từ Gemini: {Response}", geminiResponse);
            return GetFallbackSuggestion(request);
        }
    }

    /// <summary>
    /// Fallback suggestion nếu không parse được từ Gemini
    /// </summary>
    private (decimal SuggestedPrice, decimal ConfidenceScore, string? Factors, string? Analysis) 
        GetFallbackSuggestion(PriceSuggestionRequest request)
    {
        // Tính giá cơ bản dựa trên thông số (fallback logic)
        var basePrice = 10000000m; // 10 triệu VND
        var yearFactor = (DateTime.Now.Year - request.ProductionYear) * 500000m;
        var batteryFactor = request.BatteryCapacityCurrent * 2000000m;
        
        var suggestedPrice = basePrice + batteryFactor - yearFactor;
        if (suggestedPrice < 1000000) suggestedPrice = 1000000; // Tối thiểu 1 triệu

        var factors = new
        {
            brand = "Thương hiệu ảnh hưởng đến giá",
            model = "Model ảnh hưởng đến giá",
            year = $"Năm sản xuất {request.ProductionYear}",
            battery = $"Dung lượng pin {request.BatteryCapacityCurrent}",
            condition = $"Tình trạng: {request.Condition}",
            location = $"Vị trí: {request.Location}",
            marketTrend = "Xu hướng thị trường hiện tại"
        };

        return (
            suggestedPrice,
            50m, // Confidence thấp vì là fallback
            JsonSerializer.Serialize(factors),
            "Giá được tính toán dựa trên công thức cơ bản. Vui lòng kiểm tra lại với chuyên gia."
        );
    }

    // Helper classes để deserialize Gemini response
    private class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private class GeminiContent
    {
        public GeminiPart[]? Parts { get; set; }
    }

    private class GeminiPart
    {
        public string? Text { get; set; }
    }

    private class GeminiPriceSuggestion
    {
        public decimal SuggestedPrice { get; set; }
        public decimal ConfidenceScore { get; set; }
        public Dictionary<string, string>? Factors { get; set; }
        public string? Analysis { get; set; }
    }
}

