using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class MarketData : BaseEntity
{
    public Guid DataId => Id;
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? SohPercentage { get; set; }
    public int? Mileage { get; set; }
    public decimal SellingPrice { get; set; }
    public string? Location { get; set; }
    public DateTime? TransactionDate { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
}

