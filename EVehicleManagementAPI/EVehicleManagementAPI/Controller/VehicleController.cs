using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Vehicles.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        public class CreateVehicleRequest
        {
            [Required]
            public int MemberId { get; set; }
            public int? VehicleModelId { get; set; }
            public string VIN { get; set; }
            public int ManufactureYear { get; set; }
            public int MileageKm { get; set; }
            public decimal BatteryCapacity { get; set; }
            [Required]
            public string Condition { get; set; }
            [Required]
            public string Description { get; set; }
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateVehicleRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var v = new Vehicle
            {
                MemberId = req.MemberId,
                VehicleModelId = req.VehicleModelId,
                VIN = req.VIN,
                ManufactureYear = req.ManufactureYear,
                MileageKm = req.MileageKm,
                BatteryCapacity = req.BatteryCapacity,
                Condition = req.Condition,
                Description = req.Description
            };

            _context.Vehicles.Add(v);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Vehicle v)
        {
            var existing = _context.Vehicles.Find(id);
            if (existing == null) return NotFound();

            existing.VIN = v.VIN;
            existing.ManufactureYear = v.ManufactureYear;
            existing.MileageKm = v.MileageKm;
            existing.BatteryCapacity = v.BatteryCapacity;
            existing.Condition = v.Condition;
            existing.Description = v.Description;
            existing.MemberId = v.MemberId;

            _context.SaveChanges();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null) return NotFound();

            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();
            return Ok();
        }
    }
}
