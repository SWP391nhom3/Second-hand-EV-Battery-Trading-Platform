using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public RoleController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.Roles
                .Include(r => r.Accounts)
                .ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Accounts)
                .FirstOrDefaultAsync(r => r.RoleId == id);
            
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var role = await _context.Roles
                .Include(r => r.Accounts)
                .FirstOrDefaultAsync(r => r.Name == name);
            
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var roles = await _context.Roles
                .Include(r => r.Accounts)
                .Where(r => r.Status == status)
                .ToListAsync();
            
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            role.Status = "ACTIVE"; // Default status
            
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Role role)
        {
            var existing = await _context.Roles.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = role.Name;
            existing.Status = role.Status;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRoleStatusRequest request)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            role.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(role);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            // Check if role is being used by any accounts
            var accountsUsingRole = await _context.Accounts.AnyAsync(a => a.RoleId == id);
            if (accountsUsingRole)
            {
                return BadRequest(new { message = "Cannot delete role that is being used by accounts" });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/accounts")]
        public async Task<IActionResult> GetRoleAccounts(int id)
        {
            var accounts = await _context.Accounts
                .Include(a => a.Member)
                .Where(a => a.RoleId == id)
                .ToListAsync();
            
            return Ok(accounts);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalRoles = await _context.Roles.CountAsync();
            
            var rolesByStatus = await _context.Roles
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var accountsByRole = await _context.Roles
                .GroupJoin(_context.Accounts, r => r.RoleId, a => a.RoleId, (role, accounts) => new
                {
                    RoleName = role.Name,
                    AccountCount = accounts.Count()
                })
                .ToListAsync();

            var activeRoles = await _context.Roles
                .Where(r => r.Status == "ACTIVE")
                .ToListAsync();

            return Ok(new
            {
                totalRoles,
                rolesByStatus,
                accountsByRole,
                activeRoles
            });
        }

        [HttpPost("initialize-default")]
        public async Task<IActionResult> InitializeDefaultRoles()
        {
            var existingRoles = await _context.Roles.ToListAsync();
            if (existingRoles.Any())
            {
                return BadRequest(new { message = "Roles already initialized" });
            }

            var defaultRoles = new List<Role>
            {
                new Role { Name = "Admin", Status = "ACTIVE" },
                new Role { Name = "Member", Status = "ACTIVE" },
                new Role { Name = "Moderator", Status = "ACTIVE" },
                new Role { Name = "Guest", Status = "ACTIVE" }
            };

            _context.Roles.AddRange(defaultRoles);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Default roles initialized successfully", roles = defaultRoles });
        }

        [HttpGet("permissions/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound();

            // Define permissions based on role
            var permissions = new Dictionary<string, object>();

            switch (role.Name.ToLower())
            {
                case "admin":
                    permissions = new Dictionary<string, object>
                    {
                        { "canManageUsers", true },
                        { "canManageRoles", true },
                        { "canManagePosts", true },
                        { "canManagePayments", true },
                        { "canManageConstructs", true },
                        { "canViewAnalytics", true },
                        { "canModerate", true }
                    };
                    break;
                case "moderator":
                    permissions = new Dictionary<string, object>
                    {
                        { "canManageUsers", false },
                        { "canManageRoles", false },
                        { "canManagePosts", true },
                        { "canManagePayments", false },
                        { "canManageConstructs", true },
                        { "canViewAnalytics", true },
                        { "canModerate", true }
                    };
                    break;
                case "member":
                    permissions = new Dictionary<string, object>
                    {
                        { "canManageUsers", false },
                        { "canManageRoles", false },
                        { "canManagePosts", true },
                        { "canManagePayments", false },
                        { "canManageConstructs", false },
                        { "canViewAnalytics", false },
                        { "canModerate", false }
                    };
                    break;
                default:
                    permissions = new Dictionary<string, object>
                    {
                        { "canManageUsers", false },
                        { "canManageRoles", false },
                        { "canManagePosts", false },
                        { "canManagePayments", false },
                        { "canManageConstructs", false },
                        { "canViewAnalytics", false },
                        { "canModerate", false }
                    };
                    break;
            }

            return Ok(new { role = role.Name, permissions });
        }
    }

    public class UpdateRoleStatusRequest
    {
        public string Status { get; set; }
    }
}
