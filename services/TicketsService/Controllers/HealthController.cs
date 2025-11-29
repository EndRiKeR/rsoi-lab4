using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsService.Database;

namespace TicketsService.Controllers
{
    [ApiController]
    [Route("manage")]
    public class HealthController : ControllerBase
    {
        private readonly TicketsContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TicketsContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                _logger.LogInformation("Health check is started.");
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogInformation("Health check said u can't connect to DB =(((");
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        checks = new
                        {
                            database = new { status = "Unhealthy", error = "Cannot connect to database" }
                        }
                    });
                }
                
                _logger.LogInformation("Health check said u can connect to DB.");
                _logger.LogInformation("Health check cheks cols in DB.");
                var ticketsCount = await _dbContext.Tickets.CountAsync();
                _logger.LogInformation("Health check said all ok.");
                var result = new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    checks = new
                    {
                        database = new { status = "Healthy", ticketsCount = ticketsCount }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed.");
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    error = ex.Message
                });
            }
        }
    }
}