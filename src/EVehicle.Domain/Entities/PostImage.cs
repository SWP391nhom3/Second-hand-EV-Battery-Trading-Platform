using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class PostImage : BaseEntity
{
    public Guid PostId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; } = false;
    public bool IsProof { get; set; } = false; // Đánh dấu ảnh này là 'bằng chứng' SOH/KM
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Post Post { get; set; } = null!;
}

