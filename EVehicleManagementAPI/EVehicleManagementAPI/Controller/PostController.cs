using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public PostController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Include(p => p.PostRequests)
                .FirstOrDefaultAsync(p => p.PostId == id);
            
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var posts = await _context.Posts
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Where(p => p.MemberId == memberId)
                .ToListAsync();
            return Ok(posts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            post.CreatedAt = DateTime.Now;
            post.UpdatedAt = DateTime.Now;
            
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = post.PostId }, post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Post post)
        {
            var existing = await _context.Posts.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = post.Title;
            existing.Description = post.Description;
            existing.Price = post.Price;
            existing.PostType = post.PostType;
            existing.Status = post.Status;
            existing.VehicleId = post.VehicleId;
            existing.BatteryId = post.BatteryId;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedPosts()
        {
            var featuredPosts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Where(p => p.Featured == true && p.Status == "ACTIVE")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(featuredPosts);
        }
    }
}
