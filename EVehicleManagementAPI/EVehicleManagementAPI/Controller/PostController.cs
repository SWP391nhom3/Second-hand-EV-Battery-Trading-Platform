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

        // ✅ Lấy tất cả bài đăng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Include(p => p.Member).ThenInclude(m => m.Account)
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Include(p => p.Staff)
                .ToListAsync();
            return Ok(posts);
        }

        // ✅ Lấy bài đăng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Member).ThenInclude(m => m.Account)
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Include(p => p.Staff)
                .Include(p => p.PostRequests)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();
            return Ok(post);
        }

        // ✅ Lấy bài đăng theo thành viên
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var posts = await _context.Posts
                .Include(p => p.Vehicle)
                .Include(p => p.Battery)
                .Include(p => p.Staff)
                .Where(p => p.MemberId == memberId)
                .ToListAsync();
            return Ok(posts);
        }

        // ✅ Tạo bài đăng
        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            post.CreatedAt = DateTime.Now;
            post.UpdatedAt = DateTime.Now;

            // ✅ Logic xác định loại giao dịch
            if (post.PostType?.ToLower() == "e-vehicle" || post.PostType?.ToLower() == "xe điện")
            {
                post.TransactionType = "STAFF_ASSISTED";
                post.Status = "PENDING_ASSIGN"; // chờ admin gán nhân viên
            }
            else
            {
                post.TransactionType = "DIRECT";
                post.Status = "ACTIVE";
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = post.PostId }, post);
        }

        // ✅ Admin gán nhân viên cho bài đăng
        [HttpPut("{postId}/assign-staff/{staffId}")]
        public async Task<IActionResult> AssignStaff(int postId, int staffId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound("Không tìm thấy bài đăng.");

            // ✅ Nạp Member + Account để kiểm tra RoleId
            var staff = await _context.Members
                .Include(m => m.Account)
                .FirstOrDefaultAsync(m => m.MemberId == staffId);

            if (staff == null)
                return BadRequest("Không tìm thấy nhân viên.");

            if (staff.Account == null || staff.Account.RoleId != 3) // 3 = Staff
                return BadRequest("Nhân viên không hợp lệ (RoleId != 3).");

            post.StaffId = staffId;
            post.Status = "IN_PROGRESS"; // đang được xử lý
            post.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        // ✅ Cập nhật bài đăng
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

        // ✅ Xóa bài đăng
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ✅ Lấy bài đăng nổi bật
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

        // ✅ Lấy bài đăng giao dịch trực tiếp
        [HttpGet("direct")]
        public async Task<IActionResult> GetDirectPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Member)
                .Where(p => p.TransactionType == "DIRECT" && p.Status == "ACTIVE")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(posts);
        }

        // ✅ Lấy bài đăng cần nhân viên hỗ trợ
        [HttpGet("staff-assisted")]
        public async Task<IActionResult> GetStaffAssistedPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Staff)
                .Where(p => p.TransactionType == "STAFF_ASSISTED")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(posts);
        }
    }
}
