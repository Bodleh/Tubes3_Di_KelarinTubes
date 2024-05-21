using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BMController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<BMController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BMController(DatabaseContext context, ILogger<BMController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private string ConvertToBinaryString(byte[] data)
        {
            return string.Join(string.Empty, data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SearchRequest request)
        {
            if (request == null || request.Data == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            _logger.LogInformation("Received a POST request to BM endpoint");
            _logger.LogInformation("Binary data length: " + request.Data.Length);

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5099/api/sidikjari");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Error fetching data from api/sidikjari" });
            }

            var fetchResult = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(fetchResult))
            {
                return StatusCode((int)response.StatusCode, new { message = "Bad Request" });
            }

            var sidikJariList = JsonSerializer.Deserialize<List<SidikJari>>(fetchResult);
            if (sidikJariList == null)
            {
                return StatusCode((int)response.StatusCode, new { message = "Bad Request" });
            }

            foreach (var item in sidikJariList)
            {
                if (!string.IsNullOrEmpty(item.BerkasCitra))
                {
                    var filePath = item.BerkasCitra;

                    var realpath = "../" + "../" + filePath;

                    if (System.IO.File.Exists(realpath))
                    {
                        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(realpath);
                        item.BerkasCitra = ConvertToBinaryString(fileBytes);
                    }
                    else
                    {
                        item.BerkasCitra = null;
                    }
                }
            }

            return Ok(new SearchResult { SidikJari = sidikJariList[0] });
        }
    }
}
