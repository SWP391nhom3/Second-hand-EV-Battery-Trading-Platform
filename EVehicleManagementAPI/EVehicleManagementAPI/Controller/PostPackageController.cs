using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostPackageController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public PostPackageController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _context.PostPackages
                .Include(p => p.PostPackageSubs)
                .OrderBy(p => p.PriorityLevel)
                .ToListAsync();
            return Ok(packages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var package = await _context.PostPackages
                .Include(p => p.PostPackageSubs)
                .FirstOrDefaultAsync(p => p.PackageId == id);
            
            if (package == null) return NotFound();
            return Ok(package);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostPackage package)
        {
            _context.PostPackages.Add(package);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = package.PackageId }, package);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PostPackage package)
        {
            var existing = await _context.PostPackages.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = package.Name;
            existing.DurationDay = package.DurationDay;
            existing.Price = package.Price;
            existing.PriorityLevel = package.PriorityLevel;
            existing.Description = package.Description;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var package = await _context.PostPackages.FindAsync(id);
            if (package == null) return NotFound();

            _context.PostPackages.Remove(package);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePackages()
        {
            var packages = await _context.PostPackages
                .OrderBy(p => p.PriorityLevel)
                .ThenBy(p => p.Price)
                .ToListAsync();
            return Ok(packages);
        }

        [HttpGet("{id}/subscriptions")]
        public async Task<IActionResult> GetPackageSubscriptions(int id)
        {
            var subscriptions = await _context.PostPackageSubs
                .Include(ps => ps.Post)
                .Include(ps => ps.Member)
                .Include(ps => ps.Payment)
                .Where(ps => ps.PackageId == id)
                .OrderByDescending(ps => ps.StartDate)
                .ToListAsync();
            
            return Ok(subscriptions);
        }

        [HttpPost("{packageId}/subscribe")]
        public async Task<IActionResult> SubscribeToPackage(int packageId, [FromBody] SubscribeToPackageRequest request)
        {
            var package = await _context.PostPackages.FindAsync(packageId);
            if (package == null) return NotFound("Package not found");

            var post = await _context.Posts.FindAsync(request.PostId);
            if (post == null) return NotFound("Post not found");

            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null) return NotFound("Member not found");

            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(package.DurationDay);

            var subscription = new PostPackageSub
            {
                PostId = request.PostId,
                PackageId = packageId,
                MemberId = request.MemberId,
                StartDate = startDate,
                EndDate = endDate,
                PaymentId = request.PaymentId,
                Status = "ACTIVE"
            };

            _context.PostPackageSubs.Add(subscription);

            // Update post to be featured if it's a high priority package
            if (package.PriorityLevel >= 3)
            {
                post.Featured = true;
                post.ExpiryDate = endDate;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subscription created successfully", subscription });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalPackages = await _context.PostPackages.CountAsync();
            var totalSubscriptions = await _context.PostPackageSubs.CountAsync();
            
            var packagesByPriority = await _context.PostPackages
                .GroupBy(p => p.PriorityLevel)
                .Select(g => new { PriorityLevel = g.Key, Count = g.Count() })
                .OrderBy(x => x.PriorityLevel)
                .ToListAsync();

            var revenueByPackage = await _context.PostPackageSubs
                .Include(ps => ps.PostPackage)
                .GroupBy(ps => ps.PostPackage.Name)
                .Select(g => new { PackageName = g.Key, Revenue = g.Sum(ps => ps.PostPackage.Price) })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return Ok(new
            {
                totalPackages,
                totalSubscriptions,
                packagesByPriority,
                revenueByPackage
            });
        }
    }

    public class SubscribeToPackageRequest
    {
        public int PostId { get; set; }
        public int MemberId { get; set; }
        public int PaymentId { get; set; }
    }
}
