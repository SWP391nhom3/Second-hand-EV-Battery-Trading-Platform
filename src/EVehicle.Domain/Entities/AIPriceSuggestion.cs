using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class AIPriceSuggestion : BaseEntity
{
    public Guid SuggestionId => Id;
    public Guid PostId { get; set; }
    public decimal SuggestedPrice { get; set; }
    public decimal? ConfidenceScore { get; set; } // Độ tin cậy (0-100)
    public string? Factors { get; set; } // Các yếu tố ảnh hưởng (dạng JSON)

    // Navigation properties
    public Post Post { get; set; } = null!;
}

