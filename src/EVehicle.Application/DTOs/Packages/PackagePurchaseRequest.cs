namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Request DTO cho mua gói tin (UC26)
/// </summary>
public class PackagePurchaseRequest
{
    /// <summary>
    /// ID gói tin muốn mua
    /// </summary>
    public int PackageId { get; set; }

    /// <summary>
    /// Phương thức thanh toán (PAYOS)
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;
}

