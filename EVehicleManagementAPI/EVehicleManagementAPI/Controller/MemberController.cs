using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public MemberController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await _context.Members
                .Include(m => m.Account)
                .Include(m => m.Vehicles)
                .Include(m => m.Batteries)
                .ToListAsync();
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var member = await _context.Members
                .Include(m => m.Account)
                .Include(m => m.Vehicles)
                .Include(m => m.Batteries)
                .Include(m => m.Posts)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            
            if (member == null) return NotFound();
            return Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Member member)
        {
            member.JoinedAt = DateTime.Now;
            member.Rating = 0;
            
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = member.MemberId }, member);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Member member)
        {
            var existing = await _context.Members.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = member.FullName;
            existing.AvatarUrl = member.AvatarUrl;
            existing.Address = member.Address;
            existing.Status = member.Status;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedMembers()
        {
            var topMembers = await _context.Members
                .Include(m => m.Account)
                .OrderByDescending(m => m.Rating)
                .Take(10)
                .ToListAsync();
            return Ok(topMembers);
        }
    }
}
