namespace EVehicle.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty; // 'Xe điện', 'Pin'
    public string Code { get; set; } = string.Empty; // 'ELECTRIC_VEHICLE', 'BATTERY'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<ContractTemplate> ContractTemplates { get; set; } = new List<ContractTemplate>();
    public ICollection<MarketData> MarketData { get; set; } = new List<MarketData>();
}

