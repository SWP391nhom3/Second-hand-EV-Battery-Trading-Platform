using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostRequestController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public PostRequestController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.PostRequests
                .Include(pr => pr.Post)
                    .ThenInclude(p => p.Member)
                .Include(pr => pr.Buyer)
                .Include(pr => pr.Construct)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _context.PostRequests
                .Include(pr => pr.Post)
                    .ThenInclude(p => p.Member)
                .Include(pr => pr.Buyer)
                .Include(pr => pr.Construct)
                .FirstOrDefaultAsync(pr => pr.Id == id);
            
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPostId(int postId)
        {
            var requests = await _context.PostRequests
                .Include(pr => pr.Buyer)
                .Include(pr => pr.Construct)
                .Where(pr => pr.PostId == postId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
            
            return Ok(requests);
        }

        [HttpGet("buyer/{buyerId}")]
        public async Task<IActionResult> GetByBuyerId(int buyerId)
        {
            var requests = await _context.PostRequests
                .Include(pr => pr.Post)
                    .ThenInclude(p => p.Member)
                .Include(pr => pr.Construct)
                .Where(pr => pr.BuyerId == buyerId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
            
            return Ok(requests);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var requests = await _context.PostRequests
                .Include(pr => pr.Post)
                    .ThenInclude(p => p.Member)
                .Include(pr => pr.Buyer)
                .Include(pr => pr.Construct)
                .Where(pr => pr.Status == status)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
            
            return Ok(requests);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostRequest request)
        {
            request.CreatedAt = DateTime.Now;
            request.Status = "PENDING"; // Default status
            
            _context.PostRequests.Add(request);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PostRequest request)
        {
            var existing = await _context.PostRequests.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Message = request.Message;
            existing.OfferPrice = request.OfferPrice;
            existing.Status = request.Status;
            existing.ConstructId = request.ConstructId;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRequestStatusRequest request)
        {
            var postRequest = await _context.PostRequests.FindAsync(id);
            if (postRequest == null) return NotFound();

            postRequest.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(postRequest);
        }

        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.PostRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Reject all other requests for the same post
            var otherRequests = await _context.PostRequests
                .Where(pr => pr.PostId == request.PostId && pr.Id != id)
                .ToListAsync();

            foreach (var otherRequest in otherRequests)
            {
                otherRequest.Status = "REJECTED";
            }

            // Accept this request
            request.Status = "ACCEPTED";

            // Update post status to sold
            var post = await _context.Posts.FindAsync(request.PostId);
            if (post != null)
            {
                post.Status = "SOLD";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Request accepted successfully", request });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.PostRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "REJECTED";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request rejected successfully", request });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.PostRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.PostRequests.Remove(request);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalRequests = await _context.PostRequests.CountAsync();
            
            var requestsByStatus = await _context.PostRequests
                .GroupBy(pr => pr.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var averageOfferPrice = await _context.PostRequests
                .Where(pr => pr.Status == "PENDING")
                .AverageAsync(pr => (double?)pr.OfferPrice);

            var recentRequests = await _context.PostRequests
                .Include(pr => pr.Post)
                .Include(pr => pr.Buyer)
                .OrderByDescending(pr => pr.CreatedAt)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalRequests,
                requestsByStatus,
                averageOfferPrice,
                recentRequests
            });
        }

        [HttpGet("negotiations/{postId}")]
        public async Task<IActionResult> GetNegotiationsForPost(int postId)
        {
            var negotiations = await _context.PostRequests
                .Include(pr => pr.Buyer)
                .Where(pr => pr.PostId == postId && pr.Status == "PENDING")
                .OrderByDescending(pr => pr.OfferPrice)
                .ToListAsync();

            return Ok(negotiations);
        }
    }

    public class UpdateRequestStatusRequest
    {
        public string Status { get; set; }
    }
}
