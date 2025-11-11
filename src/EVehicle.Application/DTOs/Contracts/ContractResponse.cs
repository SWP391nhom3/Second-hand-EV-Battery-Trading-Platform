namespace EVehicle.Application.DTOs.Contracts;

/// <summary>
/// Response DTO cho Contract
/// </summary>
public class ContractResponse
{
    /// <summary>
    /// ID hợp đồng
    /// </summary>
    public Guid ContractId { get; set; }

    /// <summary>
    /// ID Order (nếu có)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// ID Lead (nếu có)
    /// </summary>
    public Guid? LeadId { get; set; }

    /// <summary>
    /// ID mẫu hợp đồng
    /// </summary>
    public int? ContractTemplateId { get; set; }

    /// <summary>
    /// Tên mẫu hợp đồng
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// ID Staff tạo hợp đồng
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Tên Staff tạo hợp đồng
    /// </summary>
    public string? CreatedByName { get; set; }

    /// <summary>
    /// Nội dung hợp đồng
    /// </summary>
    public string? ContractContent { get; set; }

    /// <summary>
    /// Trạng thái hợp đồng (DRAFT, PENDING_SIGNATURE, SIGNED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Đã ký bởi người mua
    /// </summary>
    public bool IsBuyerSigned { get; set; }

    /// <summary>
    /// Đã ký bởi người bán
    /// </summary>
    public bool IsSellerSigned { get; set; }

    /// <summary>
    /// Thời gian người mua ký
    /// </summary>
    public DateTime? BuyerSignedAt { get; set; }

    /// <summary>
    /// Thời gian người bán ký
    /// </summary>
    public DateTime? SellerSignedAt { get; set; }

    /// <summary>
    /// Thời gian ký hoàn tất
    /// </summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>
    /// URL file PDF hợp đồng
    /// </summary>
    public string? ContractPdfUrl { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

