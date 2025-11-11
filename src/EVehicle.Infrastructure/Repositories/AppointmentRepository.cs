using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Appointment entity
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly EVehicleDbContext _context;

    public AppointmentRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(Guid appointmentId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Lead)
                .ThenInclude(l => l.Post)
            .Include(a => a.Post)
            .Include(a => a.Buyer)
            .Include(a => a.Seller)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<List<Appointment>> GetAppointmentsByStaffIdAsync(
        Guid staffId,
        string? status = null,
        bool? upcoming = null,
        bool? past = null,
        Guid? leadId = null,
        Guid? postId = null)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Lead)
            .Include(a => a.Post)
            .Include(a => a.Buyer)
            .Include(a => a.Seller)
            .Include(a => a.Staff)
            .Where(a => a.StaffId == staffId);

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        // Filter by leadId
        if (leadId.HasValue)
        {
            query = query.Where(a => a.LeadId == leadId.Value);
        }

        // Filter by postId
        if (postId.HasValue)
        {
            query = query.Where(a => a.PostId == postId.Value);
        }

        // Filter by upcoming (StartTime >= now)
        if (upcoming == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(a => a.StartTime >= now);
        }

        // Filter by past (StartTime < now)
        if (past == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(a => a.StartTime < now);
        }

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAppointmentsByLeadIdAsync(Guid leadId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Post)
            .Include(a => a.Buyer)
            .Include(a => a.Seller)
            .Include(a => a.Staff)
            .Where(a => a.LeadId == leadId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return await Task.FromResult(appointment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

