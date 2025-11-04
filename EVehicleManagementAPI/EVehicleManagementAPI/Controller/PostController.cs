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

        // ✅ Lấy tất cả bài đăng - Sắp xếp theo PriorityLevel (gói càng cao càng ưu tiên)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Include(p => p.Member).ThenInclude(m => m.Account)
                .Include(p => p.Vehicle).ThenInclude(v => v.VehicleModel)
                .Include(p => p.Battery).ThenInclude(b => b.BatteryModel)
                .Include(p => p.Staff)
                .Include(p => p.PostPackageSubs.Where(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now))
                    .ThenInclude(ps => ps.PostPackage)
                .Where(p => p.Status == "ACTIVE")
                .ToListAsync();

            // Sắp xếp: Gói có PriorityLevel cao hơn lên trước, sau đó mới đến Featured, cuối cùng là CreatedAt
            var sortedPosts = posts.OrderByDescending(p =>
            {
                var activeSub = p.PostPackageSubs.FirstOrDefault(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now);
                return activeSub?.PostPackage?.PriorityLevel ?? 0;
            })
            .ThenByDescending(p => p.Featured)
            .ThenByDescending(p => p.CreatedAt)
            .ToList();

            return Ok(sortedPosts);
        }

        // ✅ Lấy bài đăng theo ID - Bao gồm thông tin gói
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Member).ThenInclude(m => m.Account)
                .Include(p => p.Vehicle).ThenInclude(v => v.VehicleModel)
                .Include(p => p.Battery).ThenInclude(b => b.BatteryModel)
                .Include(p => p.Staff)
                .Include(p => p.PostRequests)
                .Include(p => p.PostPackageSubs.Where(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now))
                    .ThenInclude(ps => ps.PostPackage)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();
            return Ok(post);
        }

        // ✅ Lấy bài đăng theo thành viên
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var posts = await _context.Posts
                .Include(p => p.Vehicle).ThenInclude(v => v.VehicleModel)
                .Include(p => p.Battery).ThenInclude(b => b.BatteryModel)
                .Include(p => p.Staff)
                .Where(p => p.MemberId == memberId)
                .ToListAsync();
            return Ok(posts);
        }

        // ✅ Tạo bài đăng
        // DTO: Post + VehicleModelId (optional) hoặc Vehicle data (backward compatible)
        //      + BatteryModelId (optional) hoặc Battery data (backward compatible)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
        {
            var post = new Post
            {
                MemberId = dto.MemberId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                PostType = dto.PostType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

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

            // ✅ Xử lý Vehicle: Ưu tiên VehicleModelId > Vehicle object > VehicleId trực tiếp
            if (dto.VehicleModelId.HasValue)
            {
                // Flow mới: Tự động tạo Vehicle từ model
                var vehicleModel = await _context.VehicleModels.FindAsync(dto.VehicleModelId.Value);
                if (vehicleModel == null)
                    return BadRequest("VehicleModel không tồn tại.");

                var vehicle = new Vehicle
                {
                    MemberId = dto.MemberId,
                    VehicleModelId = dto.VehicleModelId.Value,
                    Brand = vehicleModel.Brand,
                    Model = vehicleModel.Name,
                    ManufactureYear = vehicleModel.Year ?? DateTime.Now.Year,
                    BatteryCapacity = vehicleModel.Voltage ?? 0,
                    Condition = dto.VehicleCondition ?? "Good",
                    Description = vehicleModel.Description,
                    MileageKm = dto.VehicleMileageKm ?? 0
                };
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                post.VehicleId = vehicle.Id;
            }
            else if (dto.Vehicle != null)
            {
                // Backward compatible: nhận Vehicle data trực tiếp
                // Tạo Vehicle object mới để tránh validation issues với navigation properties
                var vehicle = new Vehicle
                {
                    MemberId = dto.MemberId,
                    Brand = dto.Vehicle.Brand,
                    Model = dto.Vehicle.Model,
                    ManufactureYear = dto.Vehicle.ManufactureYear,
                    MileageKm = dto.Vehicle.MileageKm,
                    BatteryCapacity = dto.Vehicle.BatteryCapacity,
                    Condition = dto.Vehicle.Condition,
                    Description = dto.Vehicle.Description
                };
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                post.VehicleId = vehicle.Id;
            }
            else if (dto.VehicleId.HasValue)
            {
                // Backward compatible: nhận VehicleId trực tiếp (để đảm bảo 100% tương thích)
                // Verify Vehicle tồn tại và thuộc về Member
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == dto.VehicleId.Value && v.MemberId == dto.MemberId);
                if (vehicle == null)
                    return BadRequest("VehicleId không tồn tại hoặc không thuộc về Member này.");
                post.VehicleId = dto.VehicleId.Value;
            }

            // ✅ Xử lý Battery: Ưu tiên BatteryModelId > Battery object > BatteryId trực tiếp
            if (dto.BatteryModelId.HasValue)
            {
                // Flow mới: Tự động tạo Battery từ model
                var batteryModel = await _context.BatteryModels.FindAsync(dto.BatteryModelId.Value);
                if (batteryModel == null)
                    return BadRequest("BatteryModel không tồn tại.");

                var battery = new Battery
                {
                    MemberId = dto.MemberId,
                    BatteryModelId = dto.BatteryModelId.Value,
                    Brand = batteryModel.Brand,
                    CapacityKWh = batteryModel.CapacityKWh ?? 0,
                    CycleCount = dto.BatteryCycleCount ?? 0,
                    ManufactureYear = DateTime.Now.Year,
                    Condition = dto.BatteryCondition ?? "Good",
                    Description = batteryModel.Description
                };
                _context.Batteries.Add(battery);
                await _context.SaveChangesAsync();
                post.BatteryId = battery.BatteryId;
            }
            else if (dto.Battery != null)
            {
                // Backward compatible: nhận Battery data trực tiếp
                // Tạo Battery object mới để tránh validation issues với navigation properties
                var battery = new Battery
                {
                    MemberId = dto.MemberId,
                    Brand = dto.Battery.Brand,
                    CapacityKWh = dto.Battery.CapacityKWh,
                    CycleCount = dto.Battery.CycleCount,
                    ManufactureYear = dto.Battery.ManufactureYear,
                    Condition = dto.Battery.Condition,
                    Description = dto.Battery.Description
                };
                _context.Batteries.Add(battery);
                await _context.SaveChangesAsync();
                post.BatteryId = battery.BatteryId;
            }
            else if (dto.BatteryId.HasValue)
            {
                // Backward compatible: nhận BatteryId trực tiếp (để đảm bảo 100% tương thích)
                // Verify Battery tồn tại và thuộc về Member
                var battery = await _context.Batteries
                    .FirstOrDefaultAsync(b => b.BatteryId == dto.BatteryId.Value && b.MemberId == dto.MemberId);
                if (battery == null)
                    return BadRequest("BatteryId không tồn tại hoặc không thuộc về Member này.");
                post.BatteryId = dto.BatteryId.Value;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // ✅ Tự động link với gói active của member (nếu có)
            var activePackageSub = await _context.PostPackageSubs
                .Include(ps => ps.PostPackage)
                .Where(ps => ps.MemberId == dto.MemberId 
                    && ps.Status == "ACTIVE" 
                    && ps.EndDate > DateTime.Now
                    && ps.PostId == null) // Chưa được gán cho post nào
                .OrderByDescending(ps => ps.PostPackage.PriorityLevel) // Ưu tiên gói cao nhất
                .FirstOrDefaultAsync();

            if (activePackageSub != null)
            {
                // Link gói với post này
                activePackageSub.PostId = post.PostId;
                
                // Set expiry date cho post dựa trên gói
                post.ExpiryDate = activePackageSub.EndDate;
                
                // Nếu gói có PriorityLevel >= 3 thì set Featured
                if (activePackageSub.PostPackage.PriorityLevel >= 3)
                {
                    post.Featured = true;
                }
                
                await _context.SaveChangesAsync();
            }

            // Load lại với model data và package info để trả về
            var createdPost = await _context.Posts
                .Include(p => p.Member).ThenInclude(m => m.Account)
                .Include(p => p.Vehicle).ThenInclude(v => v.VehicleModel)
                .Include(p => p.Battery).ThenInclude(b => b.BatteryModel)
                .Include(p => p.PostPackageSubs).ThenInclude(ps => ps.PostPackage)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            return CreatedAtAction(nameof(GetById), new { id = post.PostId }, createdPost);
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

        // ✅ Lấy bài đăng nổi bật - Sắp xếp theo PriorityLevel
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedPosts()
        {
            var featuredPosts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Vehicle).ThenInclude(v => v.VehicleModel)
                .Include(p => p.Battery).ThenInclude(b => b.BatteryModel)
                .Include(p => p.PostPackageSubs.Where(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now))
                    .ThenInclude(ps => ps.PostPackage)
                .Where(p => p.Featured == true && p.Status == "ACTIVE")
                .ToListAsync();

            // Sắp xếp theo PriorityLevel (gói cao hơn lên trước)
            var sortedPosts = featuredPosts.OrderByDescending(p =>
            {
                var activeSub = p.PostPackageSubs.FirstOrDefault(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now);
                return activeSub?.PostPackage?.PriorityLevel ?? 0;
            })
            .ThenByDescending(p => p.CreatedAt)
            .ToList();

            return Ok(sortedPosts);
        }

        // ✅ Lấy bài đăng giao dịch trực tiếp - Sắp xếp theo PriorityLevel
        [HttpGet("direct")]
        public async Task<IActionResult> GetDirectPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.PostPackageSubs.Where(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now))
                    .ThenInclude(ps => ps.PostPackage)
                .Where(p => p.TransactionType == "DIRECT" && p.Status == "ACTIVE")
                .ToListAsync();

            // Sắp xếp theo PriorityLevel (gói cao hơn lên trước)
            var sortedPosts = posts.OrderByDescending(p =>
            {
                var activeSub = p.PostPackageSubs.FirstOrDefault(ps => ps.Status == "ACTIVE" && ps.EndDate > DateTime.Now);
                return activeSub?.PostPackage?.PriorityLevel ?? 0;
            })
            .ThenByDescending(p => p.CreatedAt)
            .ToList();

            return Ok(sortedPosts);
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

    // DTO classes
    public class CreatePostDto
    {
        public int MemberId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PostType { get; set; }
        
        // Vehicle options (priority: VehicleModelId > Vehicle > VehicleId)
        public int? VehicleModelId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public int? VehicleId { get; set; }
        public string? VehicleCondition { get; set; }
        public int? VehicleMileageKm { get; set; }
        
        // Battery options (priority: BatteryModelId > Battery > BatteryId)
        public int? BatteryModelId { get; set; }
        public Battery? Battery { get; set; }
        public int? BatteryId { get; set; }
        public int? BatteryCycleCount { get; set; }
        public string? BatteryCondition { get; set; }
    }
}
