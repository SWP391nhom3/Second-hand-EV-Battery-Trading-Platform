using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Contract : BaseEntity
{
    public Guid ContractId => Id;
    public Guid? OrderId { get; set; }
    public Guid? LeadId { get; set; } // Liên kết với Lead
    public int? ContractTemplateId { get; set; }
    public Guid? CreatedBy { get; set; } // Staff tạo hợp đồng
    public string? ContractContent { get; set; } // Nội dung hợp đồng đã auto-fill
    public string? BuyerSignature { get; set; } // Chữ ký điện tử người mua
    public string? SellerSignature { get; set; } // Chữ ký điện tử người bán
    public DateTime? BuyerSignedAt { get; set; }
    public DateTime? SellerSignedAt { get; set; }
    public string? ContractPdfUrl { get; set; } // Link đến file PDF hợp đồng đã ký
    public string Status { get; set; } = "DRAFT"; // DRAFT, PENDING_SIGNATURE, SIGNED
    public DateTime? SignedAt { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Lead? Lead { get; set; }
    public ContractTemplate? ContractTemplate { get; set; }
    public User? CreatedByUser { get; set; }
}

