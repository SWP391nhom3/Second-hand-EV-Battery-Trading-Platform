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

        // 🔹 Get all payments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _context.Payments
                .Include(p => p.Buyer)
                .Include(p => p.Seller)
                .Include(p => p.Constructs)
                .Include(p => p.PostPackageSubs)
                .ToListAsync();

            return Ok(payments);
        }

        // 🔹 Get payment by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Buyer)
                .Include(p => p.Seller)
                .Include(p => p.Constructs)
                .Include(p => p.PostPackageSubs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();
            return Ok(payment);
        }

        // 🔹 Get payments by BuyerId
        [HttpGet("buyer/{buyerId}")]
        public async Task<IActionResult> GetByBuyerId(int buyerId)
        {
            var payments = await _context.Payments
                .Where(p => p.BuyerId == buyerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(payments);
        }

        // 🔹 Get payments by SellerId
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetBySellerId(int sellerId)
        {
            var payments = await _context.Payments
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(payments);
        }

        // 🔹 Get payments by Status
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var payments = await _context.Payments
                .Include(p => p.Buyer)
                .Include(p => p.Seller)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(payments);
        }

        // 🔹 Create payment
        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            payment.CreatedAt = DateTime.Now;
            payment.Status = "PENDING"; // Default

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        // 🔹 Update payment
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payment payment)
        {
            var existing = await _context.Payments.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Amount = payment.Amount;
            existing.Method = payment.Method;
            existing.TransferContent = payment.TransferContent;
            existing.Status = payment.Status;
            existing.BuyerId = payment.BuyerId;
            existing.SellerId = payment.SellerId;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // 🔹 Update payment status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(payment);
        }

        // 🔹 Delete payment
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 🔹 Statistics
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
                .Include(p => p.Buyer)
                .Include(p => p.Seller)
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

        // 🔹 Simulate payment process
        [HttpPost("process/{id}")]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            if (payment.Status != "PENDING")
            {
                return BadRequest(new { message = "Payment is not in pending status" });
            }

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
