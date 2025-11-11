namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Response DTO cho gói tin của user với số credits còn lại
/// </summary>
public class UserPackageCreditsResponse
{
    /// <summary>
    /// ID gói tin
    /// </summary>
    public int PackageId { get; set; }

    /// <summary>
    /// Tên gói tin
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Giá gói tin
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Số credits được cấp khi mua gói
    /// </summary>
    public int CreditsCount { get; set; }

    /// <summary>
    /// Số credits còn lại
    /// </summary>
    public int CreditsRemaining { get; set; }

    /// <summary>
    /// Tổng số credits đã mua
    /// </summary>
    public int TotalCredits { get; set; }

    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// Số ảnh tối đa
    /// </summary>
    public int MaxImages { get; set; }

    /// <summary>
    /// Thời gian mua gói
    /// </summary>
    public DateTime PurchasedAt { get; set; }

    /// <summary>
    /// Thời gian hết hạn (nếu có)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Có credits còn lại không
    /// </summary>
    public bool HasCredits => CreditsRemaining > 0;

    /// <summary>
    /// Đã hết hạn chưa
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}

