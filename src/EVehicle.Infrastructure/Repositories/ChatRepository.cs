using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Chat entity
/// </summary>
public class ChatRepository : IChatRepository
{
    private readonly EVehicleDbContext _context;

    public ChatRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetRoomByIdAsync(Guid roomId)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p.PostImages)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<ChatRoom?> GetRoomByPostAndBuyerAsync(Guid postId, Guid buyerId)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p.PostImages)
            .FirstOrDefaultAsync(r => r.PostId == postId && r.BuyerId == buyerId);
    }

    public async Task<ChatRoom?> GetRoomByLeadIdAsync(Guid leadId)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p.PostImages)
            .FirstOrDefaultAsync(r => r.LeadId == leadId);
    }

    public async Task<PagedResult<ChatRoom>> GetRoomsByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p.PostImages)
            .Include(r => r.ChatMessages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.User)
            .Where(r => r.BuyerId == userId || r.SellerId == userId || r.StaffId == userId);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            // Mặc định sắp xếp theo thời gian tin nhắn cuối cùng
            query = query.OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<ChatRoom>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<ChatRoom> CreateRoomAsync(ChatRoom room)
    {
        await _context.ChatRooms.AddAsync(room);
        return room;
    }

    public async Task<ChatRoom> UpdateRoomAsync(ChatRoom room)
    {
        _context.ChatRooms.Update(room);
        return await Task.FromResult(room);
    }

    public async Task<PagedResult<ChatMessage>> GetMessagesByRoomIdAsync(
        Guid roomId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.RoomId == roomId);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(GetMessageSortProperty(sortBy))
                : query.OrderBy(GetMessageSortProperty(sortBy));
        }
        else
        {
            // Mặc định sắp xếp theo thời gian tạo (mới nhất trước)
            query = query.OrderByDescending(m => m.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<ChatMessage>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        return message;
    }

    public async Task MarkMessagesAsReadAsync(Guid roomId, Guid userId)
    {
        var unreadMessages = await _context.ChatMessages
            .Where(m => m.RoomId == roomId 
                && m.UserId != userId 
                && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        _context.ChatMessages.UpdateRange(unreadMessages);
    }

    public async Task<int> GetUnreadCountAsync(Guid roomId, Guid userId)
    {
        return await _context.ChatMessages
            .CountAsync(m => m.RoomId == roomId 
                && m.UserId != userId 
                && !m.IsRead);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private static Expression<Func<ChatRoom, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "lastmessageat" => r => r.LastMessageAt ?? r.CreatedAt,
            "createdat" => r => r.CreatedAt,
            _ => r => r.LastMessageAt ?? r.CreatedAt
        };
    }

    private static Expression<Func<ChatMessage, object>> GetMessageSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "createdat" => m => m.CreatedAt,
            _ => m => m.CreatedAt
        };
    }
}

