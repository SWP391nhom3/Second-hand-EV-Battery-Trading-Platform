namespace EVehicle.Application.DTOs.Contracts;

/// <summary>
/// Response DTO cho ContractTemplate
/// </summary>
public class ContractTemplateResponse
{
    /// <summary>
    /// ID mẫu hợp đồng
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Tên mẫu hợp đồng
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung mẫu hợp đồng
    /// </summary>
    public string TemplateContent { get; set; } = string.Empty;

    /// <summary>
    /// ID danh mục (nếu có)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

