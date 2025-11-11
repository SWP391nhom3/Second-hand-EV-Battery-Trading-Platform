using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using EVehicle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVehicle.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Email Verification OTP
/// </summary>
public class EmailVerificationOtpRepository : IEmailVerificationOtpRepository
{
    private readonly EVehicleDbContext _context;

    public EmailVerificationOtpRepository(EVehicleDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationOtp> CreateAsync(EmailVerificationOtp otp)
    {
        // Xóa các OTP cũ của email này
        var existingOtps = await _context.EmailVerificationOtps
            .Where(o => o.Email.ToLower() == otp.Email.ToLower() && !o.IsUsed)
            .ToListAsync();

        _context.EmailVerificationOtps.RemoveRange(existingOtps);

        // Thêm OTP mới
        _context.EmailVerificationOtps.Add(otp);
        return otp;
    }

    public async Task<EmailVerificationOtp?> GetLatestValidOtpAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        
        return await _context.EmailVerificationOtps
            .Where(o => o.Email.ToLower() == normalizedEmail 
                && !o.IsUsed 
                && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkAsUsedAsync(Guid otpId)
    {
        var otp = await _context.EmailVerificationOtps.FindAsync(otpId);
        if (otp != null)
        {
            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;
            otp.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task IncrementAttemptCountAsync(Guid otpId)
    {
        var otp = await _context.EmailVerificationOtps.FindAsync(otpId);
        if (otp != null)
        {
            otp.AttemptCount++;
            otp.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task DeleteExpiredOtpsAsync()
    {
        var expiredOtps = await _context.EmailVerificationOtps
            .Where(o => o.ExpiresAt < DateTime.UtcNow || (o.IsUsed && o.UsedAt < DateTime.UtcNow.AddDays(-7)))
            .ToListAsync();

        _context.EmailVerificationOtps.RemoveRange(expiredOtps);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

