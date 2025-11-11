using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho AIPriceSuggestion entity
/// </summary>
public class AIPriceSuggestionRepository : IAIPriceSuggestionRepository
{
    private readonly EVehicleDbContext _context;

    public AIPriceSuggestionRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<AIPriceSuggestion> CreateAsync(AIPriceSuggestion suggestion)
    {
        await _context.AIPriceSuggestions.AddAsync(suggestion);
        return suggestion;
    }

    public async Task<AIPriceSuggestion?> GetLatestByPostIdAsync(Guid postId)
    {
        return await _context.AIPriceSuggestions
            .AsNoTracking()
            .Where(s => s.PostId == postId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


