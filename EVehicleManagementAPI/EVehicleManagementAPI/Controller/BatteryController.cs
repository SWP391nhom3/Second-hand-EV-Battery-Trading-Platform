using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteryController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public BatteryController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var batteries = await _context.Batteries
                .Include(b => b.Member)
                .ToListAsync();
            return Ok(batteries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var battery = await _context.Batteries
                .Include(b => b.Member)
                .Include(b => b.Posts)
                .FirstOrDefaultAsync(b => b.BatteryId == id);
            
            if (battery == null) return NotFound();
            return Ok(battery);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var batteries = await _context.Batteries
                .Where(b => b.MemberId == memberId)
                .ToListAsync();
            return Ok(batteries);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Battery battery)
        {
            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = battery.BatteryId }, battery);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Battery battery)
        {
            var existing = await _context.Batteries.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Brand = battery.Brand;
            existing.CapacityKWh = battery.CapacityKWh;
            existing.CycleCount = battery.CycleCount;
            existing.ManufactureYear = battery.ManufactureYear;
            existing.Condition = battery.Condition;
            existing.Description = battery.Description;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var battery = await _context.Batteries.FindAsync(id);
            if (battery == null) return NotFound();

            _context.Batteries.Remove(battery);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBatteries([FromQuery] string? brand = null, [FromQuery] decimal? minCapacity = null, [FromQuery] decimal? maxCapacity = null)
        {
            var query = _context.Batteries.AsQueryable();

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(b => b.Brand.Contains(brand));

            if (minCapacity.HasValue)
                query = query.Where(b => b.CapacityKWh >= minCapacity.Value);

            if (maxCapacity.HasValue)
                query = query.Where(b => b.CapacityKWh <= maxCapacity.Value);

            var batteries = await query.Include(b => b.Member).ToListAsync();
            return Ok(batteries);
        }
    }
}
