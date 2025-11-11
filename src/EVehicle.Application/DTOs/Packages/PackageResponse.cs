namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Response DTO cho gói tin (dùng cho select2)
/// </summary>
public class PackageResponse
{
    /// <summary>
    /// ID gói tin
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên gói tin (hiển thị)
    /// </summary>
    public string Text { get; set; } = string.Empty;

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
    /// Số credits còn lại của user (nếu có)
    /// </summary>
    public int? CreditsRemaining { get; set; }

    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// Số ảnh tối đa
    /// </summary>
    public int MaxImages { get; set; }

    /// <summary>
    /// Có credits còn lại không
    /// </summary>
    public bool HasCredits => CreditsRemaining.HasValue && CreditsRemaining.Value > 0;
}

