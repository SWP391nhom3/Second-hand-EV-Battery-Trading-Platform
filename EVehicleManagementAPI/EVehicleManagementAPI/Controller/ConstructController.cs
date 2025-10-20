using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstructController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public ConstructController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var constructs = await _context.Constructs
                .Include(c => c.Payment)
                .Include(c => c.ConstructFees)
                .Include(c => c.PostRequests)
                .ToListAsync();
            return Ok(constructs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var construct = await _context.Constructs
                .Include(c => c.Payment)
                .Include(c => c.ConstructFees)
                .Include(c => c.PostRequests)
                .FirstOrDefaultAsync(c => c.ConstructId == id);
            
            if (construct == null) return NotFound();
            return Ok(construct);
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            var constructs = await _context.Constructs
                .Include(c => c.Payment)
                .Where(c => c.Type == type)
                .ToListAsync();
            
            return Ok(constructs);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var constructs = await _context.Constructs
                .Include(c => c.Payment)
                .Where(c => c.Status == status)
                .ToListAsync();
            
            return Ok(constructs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Construct construct)
        {
            construct.Status = "ACTIVE"; // Default status
            
            _context.Constructs.Add(construct);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = construct.ConstructId }, construct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Construct construct)
        {
            var existing = await _context.Constructs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = construct.Name;
            existing.Address = construct.Address;
            existing.Contact = construct.Contact;
            existing.Type = construct.Type;
            existing.Status = construct.Status;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateConstructStatusRequest request)
        {
            var construct = await _context.Constructs.FindAsync(id);
            if (construct == null) return NotFound();

            construct.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(construct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var construct = await _context.Constructs.FindAsync(id);
            if (construct == null) return NotFound();

            _context.Constructs.Remove(construct);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/fees")]
        public async Task<IActionResult> GetConstructFees(int id)
        {
            var fees = await _context.ConstructFees
                .Include(cf => cf.Member)
                .Include(cf => cf.ServiceFee)
                .Where(cf => cf.ConstructId == id)
                .OrderByDescending(cf => cf.CreatedAt)
                .ToListAsync();
            
            return Ok(fees);
        }

        [HttpPost("{id}/fees")]
        public async Task<IActionResult> AddConstructFee(int id, [FromBody] AddConstructFeeRequest request)
        {
            var construct = await _context.Constructs.FindAsync(id);
            if (construct == null) return NotFound("Construct not found");

            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null) return NotFound("Member not found");

            var fee = new ConstructFee
            {
                ConstructId = id,
                MemberId = request.MemberId,
                ServiceName = request.ServiceName,
                Fee = request.Fee,
                CreatedAt = DateTime.Now
            };

            _context.ConstructFees.Add(fee);
            await _context.SaveChangesAsync();

            return Ok(fee);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchConstructs([FromQuery] string? name = null, [FromQuery] string? type = null, [FromQuery] string? address = null)
        {
            var query = _context.Constructs.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!string.IsNullOrEmpty(type))
                query = query.Where(c => c.Type == type);

            if (!string.IsNullOrEmpty(address))
                query = query.Where(c => c.Address.Contains(address));

            var constructs = await query
                .Include(c => c.Payment)
                .ToListAsync();

            return Ok(constructs);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalConstructs = await _context.Constructs.CountAsync();
            
            var constructsByType = await _context.Constructs
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var constructsByStatus = await _context.Constructs
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalFees = await _context.ConstructFees
                .SumAsync(cf => cf.Fee);

            var recentConstructs = await _context.Constructs
                .Include(c => c.Payment)
                .OrderByDescending(c => c.ConstructId)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalConstructs,
                constructsByType,
                constructsByStatus,
                totalFees,
                recentConstructs
            });
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyConstructs([FromQuery] string address)
        {
            // Simple implementation - in real app you'd use geolocation
            var constructs = await _context.Constructs
                .Where(c => c.Address.Contains(address))
                .Include(c => c.Payment)
                .ToListAsync();

            return Ok(constructs);
        }
    }

    public class UpdateConstructStatusRequest
    {
        public string Status { get; set; }
    }

    public class AddConstructFeeRequest
    {
        public int MemberId { get; set; }
        public string ServiceName { get; set; }
        public decimal Fee { get; set; }
    }
}
