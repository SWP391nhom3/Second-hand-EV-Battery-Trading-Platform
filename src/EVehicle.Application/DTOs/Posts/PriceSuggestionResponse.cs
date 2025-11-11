namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Response DTO cho gợi ý giá từ AI (UC51)
/// </summary>
public class PriceSuggestionResponse
{
    /// <summary>
    /// Giá được AI gợi ý (VND)
    /// </summary>
    public decimal SuggestedPrice { get; set; }

    /// <summary>
    /// Độ tin cậy (0-100%)
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// Các yếu tố ảnh hưởng đến giá (dạng JSON string)
    /// </summary>
    public string? Factors { get; set; }

    /// <summary>
    /// ID của suggestion (nếu đã lưu vào database)
    /// </summary>
    public Guid? SuggestionId { get; set; }

    /// <summary>
    /// Thông tin phân tích chi tiết từ AI (tùy chọn)
    /// </summary>
    public string? Analysis { get; set; }
}


