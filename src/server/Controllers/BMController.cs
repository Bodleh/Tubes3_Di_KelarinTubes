using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SearchRequest request)
        {
            if (request == null || request.Data == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            _logger.LogInformation("Received a POST request to BM endpoint");

            byte[] fileData;
            try
            {
                byte[] original = Convert.FromBase64String(request.Data);
                fileData = ImageConverter.ConvertImageToGrayscaleByteArray(original);
                _logger.LogInformation("originalData: " + original.Length);
                _logger.LogInformation("grayData: " + fileData.Length);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid base64 data" });
            }

            string pattern = ImageConverter.FindBestPixelSegment(fileData, 50);

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

            SidikJari? bestMatch = null;
            int bestMatchPercentage = 0;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var lockObj = new object();

            await Task.Run(() => Parallel.ForEach(sidikJariList, parallelOptions, item =>
            {
                if (!string.IsNullOrEmpty(item.BerkasCitra))
                {
                    var filePath = item.BerkasCitra;
                    var realpath = Path.Combine("../", "../", filePath);

                    if (System.IO.File.Exists(realpath))
                    {
                        byte[] originalBytes = System.IO.File.ReadAllBytes(realpath);
                        byte[] grayBytes = ImageConverter.ConvertImageToGrayscaleByteArray(originalBytes);
                        string text = ImageConverter.ConvertByteToAsciiString(grayBytes);

                        if (BoyerMoore.BMSearch(text, pattern))
                        {
                            lock (lockObj)
                            {
                                bestMatch = item;
                                bestMatchPercentage = 100;
                            }
                            parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            int distance = LevenshteinDistance.Compute(text, pattern);
                            int maxLength = Math.Max(text.Length, pattern.Length);
                            double similarity = 1.0 - (double)distance / maxLength;
                            int matchPercentage = (int)(similarity * 100);

                            lock (lockObj)
                            {
                                if (bestMatch == null || matchPercentage > bestMatchPercentage)
                                {
                                    bestMatch = item;
                                    bestMatchPercentage = matchPercentage;
                                }
                            }
                        }
                    }
                }
            }), parallelOptions.CancellationToken);

            if (bestMatch != null)
            {
                _logger.LogInformation($"Found match with {bestMatchPercentage}% similarity");
                return Ok(new SearchResult { SidikJari = bestMatch, MatchPercentage = bestMatchPercentage });
            }

            return Ok(new { message = "No matching data found" });
        }
    }
}
