namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Response DTO chi tiết cho gói tin (UC48)
/// </summary>
public class PackageDetailResponse
{
    /// <summary>
    /// ID gói tin
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên gói tin
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Giá
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Số credits được cấp
    /// </summary>
    public int CreditsCount { get; set; }

    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// Số ảnh tối đa
    /// </summary>
    public int MaxImages { get; set; }

    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}


