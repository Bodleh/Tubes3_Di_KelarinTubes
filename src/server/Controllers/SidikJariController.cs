using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SidikJariController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<SidikJariController> _logger;

        public SidikJariController(DatabaseContext context, ILogger<SidikJariController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<SidikJari> Get()
        {
            _logger.LogInformation("Fetching all SidikJari records");
            return _context.SidikJari.ToList();
        }
    }
}
