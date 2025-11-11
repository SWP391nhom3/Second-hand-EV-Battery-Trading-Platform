using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly EVehicleDbContext _context;

    public UserRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        var normalizedPhone = phoneNumber.Trim();
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
    }

    public async Task<User?> GetBySocialLoginAsync(string provider, string socialLoginId)
    {
        var normalizedProvider = provider.ToLower();
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => 
                u.SocialLoginProvider != null && 
                u.SocialLoginProvider.ToLower() == normalizedProvider &&
                u.SocialLoginId == socialLoginId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
    {
        var normalizedPhone = phoneNumber.Trim();
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == normalizedPhone);
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var trackedUser = await _context.Users.FindAsync(user.Id);
        if (trackedUser != null)
        {
            // Update các properties
            _context.Entry(trackedUser).CurrentValues.SetValues(user);
            trackedUser.UpdatedAt = DateTime.UtcNow;
            return trackedUser;
        }
        else
        {
            // Nếu không tìm thấy, attach và update
            _context.Users.Update(user);
            user.UpdatedAt = DateTime.UtcNow;
            return user;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> GetAdminsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "ADMIN")
            .ToListAsync();
    }

    public async Task<List<User>> GetStaffAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "STAFF" && u.Status == "ACTIVE")
            .ToListAsync();
    }

    public async Task<PagedResult<User>> GetUsersAsync(
        string? keyword,
        string? role,
        string? status,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Users.AsNoTracking();

        // Filter by keyword (email, phone, fullName)
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(keywordLower) ||
                u.PhoneNumber.Contains(keyword) ||
                (u.FullName != null && u.FullName.ToLower().Contains(keywordLower)));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.Status == status);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<User>.Create(items, totalCount, pageNumber, pageSize);
    }

    private static System.Linq.Expressions.Expression<Func<User, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "email" => u => u.Email,
            "fullname" => u => u.FullName ?? string.Empty,
            "role" => u => u.Role,
            "status" => u => u.Status,
            "createdat" => u => u.CreatedAt,
            _ => u => u.CreatedAt
        };
    }
}

