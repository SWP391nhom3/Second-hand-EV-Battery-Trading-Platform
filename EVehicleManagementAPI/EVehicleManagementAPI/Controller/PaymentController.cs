using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public PaymentController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _context.Payments
                .Include(p => p.Member)
                .Include(p => p.Constructs)
                .Include(p => p.PostPackageSubs)
                .ToListAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Member)
                .Include(p => p.Constructs)
                .Include(p => p.PostPackageSubs)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var payments = await _context.Payments
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(payments);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var payments = await _context.Payments
                .Include(p => p.Member)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(payments);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            payment.CreatedAt = DateTime.Now;
            payment.Status = "PENDING"; // Default status
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payment payment)
        {
            var existing = await _context.Payments.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Amount = payment.Amount;
            existing.Method = payment.Method;
            existing.TransferContent = payment.TransferContent;
            existing.Status = payment.Status;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(payment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalPayments = await _context.Payments.CountAsync();
            var totalAmount = await _context.Payments
                .Where(p => p.Status == "COMPLETED")
                .SumAsync(p => p.Amount);
            
            var paymentsByStatus = await _context.Payments
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentPayments = await _context.Payments
                .Include(p => p.Member)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalPayments,
                totalAmount,
                paymentsByStatus,
                recentPayments
            });
        }

        [HttpPost("process/{id}")]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            if (payment.Status != "PENDING")
            {
                return BadRequest(new { message = "Payment is not in pending status" });
            }

            // Simulate payment processing
            payment.Status = "COMPLETED";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment processed successfully", payment });
        }
    }

    public class UpdatePaymentStatusRequest
    {
        public string Status { get; set; }
    }
}
