using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Payment entity
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly EVehicleDbContext _context;

    public PaymentRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid paymentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetByIdWithDetailsAsync(Guid paymentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Package)
            .Include(p => p.Order)
                .ThenInclude(o => o!.Post)
            .Include(p => p.Order)
                .ThenInclude(o => o!.Seller)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetByIdForUpdateAsync(Guid paymentId)
    {
        // Không dùng AsNoTracking() vì method này được dùng để update payment
        return await _context.Payments
            .Include(p => p.Order)
            .Include(p => p.User)
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetByTransactionCodeAsync(string transactionCode)
    {
        // Không dùng AsNoTracking() vì method này được dùng trong webhook handler để update payment
        return await _context.Payments
            .Include(p => p.Order)
            .Include(p => p.User)
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment?> GetByPackageIdAndUserIdAsync(int packageId, Guid userId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Package)
            .Include(p => p.User)
            .Where(p => p.PackageId == packageId && p.UserId == userId && p.PaymentType == "PACKAGE")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Payment>> GetPaymentsByUserIdAsync(Guid userId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<Payment>> SearchPaymentsAsync(
        Guid userId,
        string? paymentType,
        string? status,
        string? paymentGateway,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Package)
            .Include(p => p.Order)
                .ThenInclude(o => o!.Post)
            .Include(p => p.Order)
                .ThenInclude(o => o!.Seller)
            .Where(p => p.UserId == userId);

        // Filter by PaymentType
        if (!string.IsNullOrWhiteSpace(paymentType))
        {
            query = query.Where(p => p.PaymentType == paymentType.ToUpper());
        }

        // Filter by Status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status.ToUpper());
        }

        // Filter by PaymentGateway
        if (!string.IsNullOrWhiteSpace(paymentGateway))
        {
            query = query.Where(p => p.PaymentGateway == paymentGateway.ToUpper());
        }

        // Filter by Date range
        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= toDate.Value.AddDays(1).AddTicks(-1)); // Include entire day
        }

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDirection);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Payment>.Create(items, totalCount, pageNumber, pageSize);
    }

    private static IQueryable<Payment> ApplySorting(
        IQueryable<Payment> query,
        string? sortBy,
        string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            // Default: sort by CreatedAt descending
            return query.OrderByDescending(p => p.CreatedAt);
        }

        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "createdat" => isDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            "amount" => isDescending
                ? query.OrderByDescending(p => p.Amount)
                : query.OrderBy(p => p.Amount),
            "status" => isDescending
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),
            "completedat" => isDescending
                ? query.OrderByDescending(p => p.CompletedAt)
                : query.OrderBy(p => p.CompletedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default
        };
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        return await Task.FromResult(payment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

