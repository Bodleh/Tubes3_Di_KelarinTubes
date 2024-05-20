using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BMController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<BMController> _logger;

        public BMController(DatabaseContext context, ILogger<BMController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post()
        {
            _logger.LogInformation("Received a POST request to BM endpoint");
            return Ok(new { message = "ok" });
        }
    }
}
