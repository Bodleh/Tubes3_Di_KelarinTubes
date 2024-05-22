using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BioSearch : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<BioSearch> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BioSearch(DatabaseContext context, ILogger<BioSearch> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private string NormalizeName(string name)
        {
            name = name.ToLower();
            name = name.Replace('0', 'o')
                    .Replace('1', 'i')
                    .Replace('2', 'a')
                    .Replace('3', 'e')
                    .Replace('4', 'a')
                    .Replace('5', 's')
                    .Replace('6', 'g')
                    .Replace('7', 't')
                    .Replace('8', 'b')
                    .Replace('9', 'g');

            name = Regex.Replace(name, "[^a-z]", "");
            return name;
        }

        private List<string> NormalizeAndSplitName(string name)
        {
            name = name.ToLower();
            name = name.Replace('0', 'o')
                    .Replace('1', 'i')
                    .Replace('2', 'a')
                    .Replace('3', 'e')
                    .Replace('4', 'a')
                    .Replace('5', 's')
                    .Replace('6', 'g')
                    .Replace('7', 't')
                    .Replace('8', 'b')
                    .Replace('9', 'g');

            name = Regex.Replace(name, "[^a-z ]", "");
            var nameParts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(nameParts);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StringRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Realname))
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            _logger.LogInformation("Received a POST request to BM endpoint");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5099/api/biodata");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Error fetching data from api/biodata" });
            }

            var fetchResult = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(fetchResult))
            {
                return StatusCode((int)response.StatusCode, new { message = "Bad Request" });
            }

            var biodataList = JsonSerializer.Deserialize<List<Biodata>>(fetchResult);
            if (biodataList == null)
            {
                return StatusCode((int)response.StatusCode, new { message = "Bad Request" });
            }

            var normalizedRealnameParts = NormalizeAndSplitName(request.Realname);
            List<Biodata> matchesList = new List<Biodata>();

            foreach (var biodata in biodataList)
            {
                if (!string.IsNullOrEmpty(biodata.Nama))
                {
                    var normalizedName = NormalizeName(biodata.Nama);

                    foreach (var part in normalizedRealnameParts)
                    {
                        if (request.IsBM==null || request.IsBM==true)
                        {   
                            if (BoyerMoore.BMSearch(normalizedName, part)) {
                                matchesList.Add(biodata);
                                break;
                            }
                        } else {
                            if (KnuthMorrisPratt.KMPSearch(normalizedName, part)) {
                                matchesList.Add(biodata);
                                break;
                            }
                        }
                    }
                }
            }

            if (matchesList.Count == 0)
            {
                return NotFound(new { message = "Biodata not found for the given name" });
            }

            var groupedMatches = matchesList
                .GroupBy(b => b)
                .Select(group => new
                {
                    Biodata = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var mostFrequentMatches = groupedMatches
                .Where(g => g.Count == groupedMatches[0].Count)
                .Select(g => g.Biodata)
                .ToList();

            Biodata? closestMatch = null;
            int smallestDistance = int.MaxValue;

            foreach (var match in mostFrequentMatches)
            {
                if (match.Nama!=null)
                {    
                    int totalDistance = LevenshteinDistance.Compute(request.Realname, match.Nama);
                    if (totalDistance < smallestDistance)
                    {
                        smallestDistance = totalDistance;
                        closestMatch = match;
                    }
                }
            }

            if (closestMatch != null)
            {
                return Ok(new { data = closestMatch });
            }
            else
            {
                return NotFound(new { message = "Biodata not found for the given name" });
            }
        }
    }
}
