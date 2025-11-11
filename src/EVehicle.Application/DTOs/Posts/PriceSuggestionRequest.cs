namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO để lấy gợi ý giá từ AI (UC51)
/// </summary>
public class PriceSuggestionRequest
{
    /// <summary>
    /// ID danh mục (Xe điện hoặc Pin)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Model
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Dung lượng pin hiện tại (kWh hoặc Ah)
    /// </summary>
    public decimal BatteryCapacityCurrent { get; set; }

    /// <summary>
    /// Số lần sạc
    /// </summary>
    public int? ChargeCount { get; set; }

    /// <summary>
    /// Năm sản xuất
    /// </summary>
    public int ProductionYear { get; set; }

    /// <summary>
    /// Tình trạng (Mới, Cũ, Đã qua sử dụng)
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Số KM đã đi (chỉ dành cho Xe điện)
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết (tùy chọn, để AI phân tích thêm)
    /// </summary>
    public string? Description { get; set; }
}


