namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Response DTO cho mua gói tin (UC26)
/// </summary>
public class PackagePurchaseResponse
{
    /// <summary>
    /// ID thanh toán
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// ID gói tin
    /// </summary>
    public int PackageId { get; set; }

    /// <summary>
    /// Tên gói tin
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Số tiền
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Cổng thanh toán (PAYOS)
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái (PENDING, SUCCESS, FAILED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// URL thanh toán (để chuyển hướng người dùng)
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// Số credits sẽ được cấp khi thanh toán thành công
    /// </summary>
    public int CreditsCount { get; set; }
}

