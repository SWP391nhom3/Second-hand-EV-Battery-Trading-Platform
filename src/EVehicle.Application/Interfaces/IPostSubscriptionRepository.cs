using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho PostSubscription Repository
/// </summary>
public interface IPostSubscriptionRepository
{
    Task<PostSubscription?> GetByPostIdAsync(Guid postId);
    Task<PostSubscription> CreateAsync(PostSubscription subscription);
    Task SaveChangesAsync();
}

