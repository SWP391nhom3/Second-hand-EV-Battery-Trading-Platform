using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public VehicleController(EVehicleDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy tất cả xe (kèm thông tin người sở hữu)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Member)
                .ToListAsync();

            return Ok(vehicles);
        }

        // 🔹 Lấy xe theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Member)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
                return NotFound(new { message = "Xe không tồn tại" });

            return Ok(vehicle);
        }

        // 🔹 Thêm mới xe
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Vehicle v)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            v.CreatedAt = DateTime.Now;
            v.Status = string.IsNullOrEmpty(v.Status) ? "AVAILABLE" : v.Status;

            _context.Vehicles.Add(v);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = v.VehicleId }, v);
        }

        // 🔹 Cập nhật xe
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vehicle v)
        {
            if (id != v.VehicleId)
                return BadRequest(new { message = "ID không khớp" });

            var existing = await _context.Vehicles.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Xe không tồn tại" });

            existing.Brand = v.Brand;
            existing.Model = v.Model;
            existing.ManufactureYear = v.ManufactureYear;
            existing.MileageKm = v.MileageKm;
            existing.BatteryCapacity = v.BatteryCapacity;
            existing.Condition = v.Condition;
            existing.Description = v.Description;
            existing.Price = v.Price;
            existing.Status = v.Status;
            existing.ImageUrl = v.ImageUrl;
            existing.MemberId = v.MemberId;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // 🔹 Xóa xe
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound(new { message = "Xe không tồn tại" });

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa xe thành công" });
        }
    }
}
