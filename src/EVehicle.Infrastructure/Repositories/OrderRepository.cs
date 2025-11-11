using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Order entity
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly EVehicleDbContext _context;

    public OrderRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Lead)
            .Include(o => o.Post)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.Staff)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> GetByLeadIdAsync(Guid leadId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Post)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.LeadId == leadId);
    }

    public async Task<List<Order>> GetOrdersByBuyerIdAsync(Guid buyerId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Post)
            .Include(o => o.Seller)
            .Include(o => o.Payments)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersBySellerIdAsync(Guid sellerId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Post)
            .Include(o => o.Buyer)
            .Include(o => o.Payments)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        return await Task.FromResult(order);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

