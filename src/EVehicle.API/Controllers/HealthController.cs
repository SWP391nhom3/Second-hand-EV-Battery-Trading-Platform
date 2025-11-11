using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVehicle.Infrastructure.Data;

namespace EVehicle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly EVehicleDbContext _context;

    public HealthController(EVehicleDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connection
            var canConnect = await _context.Database.CanConnectAsync();
            
            return Ok(new
            {
                status = "healthy",
                database = canConnect ? "connected" : "disconnected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

