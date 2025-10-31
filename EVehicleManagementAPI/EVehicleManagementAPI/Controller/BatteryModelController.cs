using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteryModelController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public BatteryModelController(EVehicleDbContext context)
        {
            _context = context;
        }

        // GET: api/BatteryModel/list?brand=CATL&chemistry=LFP&minCapacity=50&maxCapacity=100
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? brand,
            [FromQuery] string? chemistry,
            [FromQuery] decimal? minVoltage,
            [FromQuery] decimal? maxVoltage,
            [FromQuery] decimal? minCapacityKWh,
            [FromQuery] decimal? maxCapacityKWh,
            [FromQuery] decimal? minAmperage,
            [FromQuery] decimal? maxAmperage,
            [FromQuery] string? formFactor,
            [FromQuery] int? minCycles,
            [FromQuery] int? maxCycles,
            [FromQuery] bool? isCustom,
            [FromQuery] bool? isApproved,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.BatteryModels.AsQueryable();

            // Áp dụng các filter
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(bm => bm.Brand.Contains(brand));
            if (!string.IsNullOrEmpty(chemistry))
                query = query.Where(bm => bm.Chemistry.Contains(chemistry));
            if (minVoltage.HasValue)
                query = query.Where(bm => bm.Voltage >= minVoltage);
            if (maxVoltage.HasValue)
                query = query.Where(bm => bm.Voltage <= maxVoltage);
            if (minCapacityKWh.HasValue)
                query = query.Where(bm => bm.CapacityKWh >= minCapacityKWh);
            if (maxCapacityKWh.HasValue)
                query = query.Where(bm => bm.CapacityKWh <= maxCapacityKWh);
            if (minAmperage.HasValue)
                query = query.Where(bm => bm.Amperage >= minAmperage);
            if (maxAmperage.HasValue)
                query = query.Where(bm => bm.Amperage <= maxAmperage);
            if (!string.IsNullOrEmpty(formFactor))
                query = query.Where(bm => bm.FormFactor.Contains(formFactor));
            if (minCycles.HasValue)
                query = query.Where(bm => bm.Cycles >= minCycles);
            if (maxCycles.HasValue)
                query = query.Where(bm => bm.Cycles <= maxCycles);
            if (isCustom.HasValue)
                query = query.Where(bm => bm.IsCustom == isCustom);
            if (isApproved.HasValue)
                query = query.Where(bm => bm.IsApproved == isApproved);

            var total = await query.CountAsync();
            var models = await query
                .OrderByDescending(bm => bm.CreatedAt)
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

        // GET: api/BatteryModel/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _context.BatteryModels
                .Include(bm => bm.Batteries)
                .FirstOrDefaultAsync(bm => bm.BatteryModelId == id);

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        // POST: api/BatteryModel/custom
        // Cho phép user submit model mới (IsCustom = true, IsApproved = false)
        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustom([FromBody] BatteryModel model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Brand))
                return BadRequest("Name và Brand là bắt buộc.");

            // Kiểm tra duplicate: cùng Brand, Name, Chemistry, Capacity
            var existing = await _context.BatteryModels
                .FirstOrDefaultAsync(bm =>
                    bm.Brand.ToLower() == model.Brand.ToLower() &&
                    bm.Name.ToLower() == model.Name.ToLower() &&
                    bm.Chemistry.ToLower() == model.Chemistry.ToLower() &&
                    bm.CapacityKWh == model.CapacityKWh);

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

            _context.BatteryModels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.BatteryModelId }, model);
        }

        // GET: api/BatteryModel/all-filters
        // Trả về các giá trị distinct để FE build dropdowns
        [HttpGet("all-filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            var brands = await _context.BatteryModels
                .Where(bm => !bm.IsCustom || bm.IsApproved) // Chỉ lấy model chuẩn hoặc đã approved
                .Select(bm => bm.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            var chemistries = await _context.BatteryModels
                .Where(bm => !string.IsNullOrEmpty(bm.Chemistry) && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.Chemistry)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var formFactors = await _context.BatteryModels
                .Where(bm => !string.IsNullOrEmpty(bm.FormFactor) && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.FormFactor)
                .Distinct()
                .OrderBy(ff => ff)
                .ToListAsync();

            var minMaxVoltage = await _context.BatteryModels
                .Where(bm => bm.Voltage.HasValue && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.Voltage!.Value)
                .ToListAsync();

            var minMaxCapacity = await _context.BatteryModels
                .Where(bm => bm.CapacityKWh.HasValue && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.CapacityKWh!.Value)
                .ToListAsync();

            var minMaxAmperage = await _context.BatteryModels
                .Where(bm => bm.Amperage.HasValue && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.Amperage!.Value)
                .ToListAsync();

            var minMaxCycles = await _context.BatteryModels
                .Where(bm => bm.Cycles.HasValue && (!bm.IsCustom || bm.IsApproved))
                .Select(bm => bm.Cycles!.Value)
                .ToListAsync();

            return Ok(new
            {
                brands,
                chemistries,
                formFactors,
                voltage = minMaxVoltage.Any() ? new { min = minMaxVoltage.Min(), max = minMaxVoltage.Max() } : null,
                capacityKWh = minMaxCapacity.Any() ? new { min = minMaxCapacity.Min(), max = minMaxCapacity.Max() } : null,
                amperage = minMaxAmperage.Any() ? new { min = minMaxAmperage.Min(), max = minMaxAmperage.Max() } : null,
                cycles = minMaxCycles.Any() ? new { min = minMaxCycles.Min(), max = minMaxCycles.Max() } : null
            });
        }

        // GET: api/BatteryModel/search?q=CATL
        // Suggest models khi user nhập tên/hãng
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrEmpty(q))
                return BadRequest("Query string không được để trống.");

            var models = await _context.BatteryModels
                .Where(bm => (!bm.IsCustom || bm.IsApproved) &&
                    (bm.Name.Contains(q) || bm.Brand.Contains(q)))
                .OrderByDescending(bm => bm.IsApproved) // Ưu tiên model đã approved
                .ThenBy(bm => bm.Brand)
                .ThenBy(bm => bm.Name)
                .Take(limit)
                .Select(bm => new
                {
                    bm.BatteryModelId,
                    bm.Name,
                    bm.Brand,
                    bm.Chemistry,
                    bm.CapacityKWh
                })
                .ToListAsync();

            return Ok(models);
        }
    }
}
