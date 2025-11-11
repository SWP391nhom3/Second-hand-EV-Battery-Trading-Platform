namespace EVehicle.Application.DTOs.Contracts;

/// <summary>
/// Request DTO cho tạo hợp đồng (UC43, UC29)
/// </summary>
public class ContractCreateRequest
{
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
    public int ContractTemplateId { get; set; }

    /// <summary>
    /// Nội dung hợp đồng đã chỉnh sửa (nếu có)
    /// </summary>
    public string? ContractContent { get; set; }
}

