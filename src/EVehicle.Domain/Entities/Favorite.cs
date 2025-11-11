using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid FavoriteId => Id;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}

