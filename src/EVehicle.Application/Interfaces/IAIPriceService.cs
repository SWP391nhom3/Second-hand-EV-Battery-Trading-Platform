using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho AI Price Suggestion Service (UC51)
/// </summary>
public interface IAIPriceService
{
    /// <summary>
    /// Gợi ý giá bán dựa trên thông tin sản phẩm sử dụng AI (Google Gemini)
    /// </summary>
    /// <param name="request">Thông tin sản phẩm để AI phân tích</param>
    /// <returns>Giá gợi ý, độ tin cậy và các yếu tố ảnh hưởng</returns>
    Task<BaseResponse<PriceSuggestionResponse>> SuggestPriceAsync(PriceSuggestionRequest request);
}


