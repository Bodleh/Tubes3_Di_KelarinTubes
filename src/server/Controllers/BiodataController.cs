using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiodataController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<BiodataController> _logger;

        public BiodataController(DatabaseContext context, ILogger<BiodataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<EncryptedBiodata> Get()
        {
            _logger.LogInformation("Fetching all Biodata records");
            return _context.EncryptedBiodata.ToList();
        }
    }
}
