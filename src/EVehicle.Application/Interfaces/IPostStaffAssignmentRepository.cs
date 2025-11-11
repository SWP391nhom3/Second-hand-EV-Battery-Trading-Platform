using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho PostStaffAssignment Repository
/// </summary>
public interface IPostStaffAssignmentRepository
{
    Task<PostStaffAssignment?> GetByPostIdAsync(Guid postId);
    Task<PostStaffAssignment> CreateAsync(PostStaffAssignment assignment);
    Task SaveChangesAsync();
}

