using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly EVehicleDbContext _context;

        public AccountController(EVehicleDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .ToListAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.AccountId == id);
            
            if (account == null) return NotFound();
            return Ok(account);
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var account = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Email == email);
            
            if (account == null) return NotFound();
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Account account)
        {
            account.CreatedAt = DateTime.Now;
            
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = account.AccountId }, account);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Account account)
        {
            var existing = await _context.Accounts.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Email = account.Email;
            existing.Phone = account.Phone;
            existing.PasswordHash = account.PasswordHash;
            existing.RoleId = account.RoleId;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetByRole(int roleId)
        {
            var accounts = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Member)
                .Where(a => a.RoleId == roleId)
                .ToListAsync();
            return Ok(accounts);
        }
    }
}
