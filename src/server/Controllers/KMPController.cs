using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KMPController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<KMPController> _logger;

        public KMPController(DatabaseContext context, ILogger<KMPController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post()
        {
            _logger.LogInformation("Received a POST request to KMP endpoint");
            return Ok(new { message = "ok" });
        }
    }
}
