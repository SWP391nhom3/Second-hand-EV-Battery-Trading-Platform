namespace EVehicle.Domain.Entities;

public class ContractTemplate
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty; // 'Hợp đồng mua bán xe điện', 'Hợp đồng mua bán pin'
    public string TemplateContent { get; set; } = string.Empty; // Nội dung mẫu hợp đồng với các placeholder
    public int? CategoryId { get; set; } // Áp dụng cho danh mục nào
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}

