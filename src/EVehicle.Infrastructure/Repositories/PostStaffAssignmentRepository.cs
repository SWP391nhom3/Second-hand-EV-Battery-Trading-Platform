using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho PostStaffAssignment entity
/// </summary>
public class PostStaffAssignmentRepository : IPostStaffAssignmentRepository
{
    private readonly EVehicleDbContext _context;

    public PostStaffAssignmentRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<PostStaffAssignment?> GetByPostIdAsync(Guid postId)
    {
        return await _context.PostStaffAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(psa => psa.PostId == postId && psa.IsActive);
    }

    public async Task<PostStaffAssignment> CreateAsync(PostStaffAssignment assignment)
    {
        await _context.PostStaffAssignments.AddAsync(assignment);
        return assignment;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

