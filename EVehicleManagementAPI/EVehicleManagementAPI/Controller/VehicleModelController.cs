using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleModelController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public VehicleModelController(EVehicleDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleModel/list?brand=VinFast&year=2023&type=SUV&minRange=200
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? brand,
            [FromQuery] int? year,
            [FromQuery] string? type,
            [FromQuery] decimal? minMotorPower,
            [FromQuery] decimal? maxMotorPower,
            [FromQuery] int? minRange,
            [FromQuery] int? maxRange,
            [FromQuery] int? minSeats,
            [FromQuery] int? maxSeats,
            [FromQuery] bool? isCustom,
            [FromQuery] bool? isApproved,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.VehicleModels.AsQueryable();

            // Áp dụng các filter
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(vm => vm.Brand.Contains(brand));
            if (year.HasValue)
                query = query.Where(vm => vm.Year == year);
            if (!string.IsNullOrEmpty(type))
                query = query.Where(vm => vm.Type.Contains(type));
            if (minMotorPower.HasValue)
                query = query.Where(vm => vm.MotorPower >= minMotorPower);
            if (maxMotorPower.HasValue)
                query = query.Where(vm => vm.MotorPower <= maxMotorPower);
            if (minRange.HasValue)
                query = query.Where(vm => vm.Range >= minRange);
            if (maxRange.HasValue)
                query = query.Where(vm => vm.Range <= maxRange);
            if (minSeats.HasValue)
                query = query.Where(vm => vm.Seats >= minSeats);
            if (maxSeats.HasValue)
                query = query.Where(vm => vm.Seats <= maxSeats);
            if (isCustom.HasValue)
                query = query.Where(vm => vm.IsCustom == isCustom);
            if (isApproved.HasValue)
                query = query.Where(vm => vm.IsApproved == isApproved);

            var total = await query.CountAsync();
            var models = await query
                .OrderByDescending(vm => vm.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = models,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // GET: api/VehicleModel/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _context.VehicleModels
                .Include(vm => vm.Vehicles)
                .FirstOrDefaultAsync(vm => vm.VehicleModelId == id);

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        // POST: api/VehicleModel/custom
        // Cho phép user submit model mới (IsCustom = true, IsApproved = false)
        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustom([FromBody] VehicleModel model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Brand))
                return BadRequest("Name và Brand là bắt buộc.");

            // Kiểm tra duplicate: cùng Brand, Name, Year, Type
            var existing = await _context.VehicleModels
                .FirstOrDefaultAsync(vm =>
                    vm.Brand.ToLower() == model.Brand.ToLower() &&
                    vm.Name.ToLower() == model.Name.ToLower() &&
                    vm.Year == model.Year &&
                    vm.Type.ToLower() == model.Type.ToLower());

            if (existing != null)
            {
                return Conflict(new
                {
                    message = "Model đã tồn tại. Vui lòng chọn model có sẵn hoặc cập nhật thông tin.",
                    existingModel = existing
                });
            }

            model.IsCustom = true;
            model.IsApproved = false;
            model.CreatedAt = DateTime.Now;

            _context.VehicleModels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.VehicleModelId }, model);
        }

        // GET: api/VehicleModel/all-filters
        // Trả về các giá trị distinct để FE build dropdowns
        [HttpGet("all-filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            var brands = await _context.VehicleModels
                .Where(vm => !vm.IsCustom || vm.IsApproved) // Chỉ lấy model chuẩn hoặc đã approved
                .Select(vm => vm.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            var types = await _context.VehicleModels
                .Where(vm => !vm.IsCustom || vm.IsApproved)
                .Select(vm => vm.Type)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t))
                .OrderBy(t => t)
                .ToListAsync();

            var years = await _context.VehicleModels
                .Where(vm => vm.Year.HasValue && (!vm.IsCustom || vm.IsApproved))
                .Select(vm => vm.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            var batteryTypes = await _context.VehicleModels
                .Where(vm => !string.IsNullOrEmpty(vm.BatteryType) && (!vm.IsCustom || vm.IsApproved))
                .Select(vm => vm.BatteryType)
                .Distinct()
                .OrderBy(bt => bt)
                .ToListAsync();

            var minMaxMotorPower = await _context.VehicleModels
                .Where(vm => vm.MotorPower.HasValue && (!vm.IsCustom || vm.IsApproved))
                .Select(vm => vm.MotorPower!.Value)
                .ToListAsync();

            var minMaxRange = await _context.VehicleModels
                .Where(vm => vm.Range.HasValue && (!vm.IsCustom || vm.IsApproved))
                .Select(vm => vm.Range!.Value)
                .ToListAsync();

            var minMaxSeats = await _context.VehicleModels
                .Where(vm => vm.Seats.HasValue && (!vm.IsCustom || vm.IsApproved))
                .Select(vm => vm.Seats!.Value)
                .ToListAsync();

            return Ok(new
            {
                brands,
                types,
                years,
                batteryTypes,
                motorPower = minMaxMotorPower.Any() ? new { min = minMaxMotorPower.Min(), max = minMaxMotorPower.Max() } : null,
                range = minMaxRange.Any() ? new { min = minMaxRange.Min(), max = minMaxRange.Max() } : null,
                seats = minMaxSeats.Any() ? new { min = minMaxSeats.Min(), max = minMaxSeats.Max() } : null
            });
        }

        // GET: api/VehicleModel/search?q=VF
        // Suggest models khi user nhập tên/hãng
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrEmpty(q))
                return BadRequest("Query string không được để trống.");

            var models = await _context.VehicleModels
                .Where(vm => (!vm.IsCustom || vm.IsApproved) &&
                    (vm.Name.Contains(q) || vm.Brand.Contains(q)))
                .OrderByDescending(vm => vm.IsApproved) // Ưu tiên model đã approved
                .ThenBy(vm => vm.Brand)
                .ThenBy(vm => vm.Name)
                .Take(limit)
                .Select(vm => new
                {
                    vm.VehicleModelId,
                    vm.Name,
                    vm.Brand,
                    vm.Year,
                    vm.Type
                })
                .ToListAsync();

            return Ok(models);
        }
    }
}
