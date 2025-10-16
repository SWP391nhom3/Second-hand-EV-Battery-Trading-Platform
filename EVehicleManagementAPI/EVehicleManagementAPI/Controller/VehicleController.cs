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

        [HttpPost]
        public IActionResult Create(Vehicle v)
        {
            _context.Vehicles.Add(v);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Vehicle v)
        {
            var existing = _context.Vehicles.Find(id);
            if (existing == null) return NotFound();

            existing.Brand = v.Brand;
            existing.Model = v.Model;
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
