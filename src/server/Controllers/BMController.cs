using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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

            byte[] fileData,  fileData2;
            int width, height, width2, height2;
            try
            {
                byte[] original = Convert.FromBase64String(request.Data);
                (fileData, width, height) = ImageConverter.ConvertImageToGrayscaleByteArray(original, 2);
                (fileData2, width2, height2) = ImageConverter.ConvertImageToGrayscaleByteArray(original);
                _logger.LogInformation("originalData: " + original.Length);
                _logger.LogInformation("grayData: " + fileData.Length);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid base64 data" });
            }

            int segmentWidth = width * 3 / 5;
            string pattern = ImageConverter.FindBestPixelSegment(fileData, width, height, segmentWidth);
            string pattern2 = ImageConverter.FindBestPixelSegment(fileData2, width2, height2, segmentWidth);
            string fullpattern = ImageConverter.ConvertByteToAsciiString(fileData2);

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
            SidikJari? bestMatchlcs = null;
            int bestMatchPercentage = 0;
            int bestMatchPercentagelcs = 0;
            var cts = new CancellationTokenSource();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cts.Token };
            var lockObj = new object();

            bool foundQuickMatch = false;

            try
            {
                await Task.Run(() => Parallel.ForEach(sidikJariList, parallelOptions, (item, state) =>
                {
                    if (!string.IsNullOrEmpty(item.BerkasCitra))
                    {
                        var filePath = item.BerkasCitra;
                        var realpath = Path.Combine("../", "../", filePath);

                        if (System.IO.File.Exists(realpath))
                        {
                            byte[] originalBytes = System.IO.File.ReadAllBytes(realpath);
                            (byte[] grayBytes, int imgWidth, int imgHeight) = ImageConverter.ConvertImageToGrayscaleByteArray(originalBytes, 2);

                            string text = ImageConverter.ConvertByteToAsciiString(grayBytes);

                            if (BoyerMoore.BMSearch(text, pattern))
                            {
                                lock (lockObj)
                                {
                                    bestMatch = item;
                                    bestMatchPercentage = 100;
                                }
                                cts.Cancel();
                                foundQuickMatch = true;
                                return;
                            }
                        }
                    }
                }), parallelOptions.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Quick match found and search canceled.");
            }

            if (!foundQuickMatch)
            {
                _logger.LogInformation("No quick match found. Performing full image search...");

                try
                {
                    await Task.Run(() => Parallel.ForEach(sidikJariList, parallelOptions, (item, state) =>
                    {
                        if (!string.IsNullOrEmpty(item.BerkasCitra))
                        {
                            var filePath = item.BerkasCitra;
                            var realpath = Path.Combine("../", "../", filePath);

                            if (System.IO.File.Exists(realpath))
                            {
                                byte[] originalBytes = System.IO.File.ReadAllBytes(realpath);
                                (byte[] grayBytes, int imgWidth, int imgHeight) = ImageConverter.ConvertImageToGrayscaleByteArray(originalBytes);

                                string text = ImageConverter.ConvertByteToAsciiString(grayBytes);

                                if (BoyerMoore.BMSearch(text, pattern2))
                                {
                                    lock (lockObj)
                                    {
                                        bestMatch = item;
                                        bestMatchPercentage = 100;
                                    }
                                    cts.Cancel();
                                    return;
                                }
                                else
                                {
                                    if (text.Length == fullpattern.Length)
                                    {
                                        int hammingDistance = Hamming.HammingDist(text, fullpattern);
                                        double similarity = (double)(text.Length - hammingDistance) / text.Length;
                                        int matchPercentage = (int)(similarity * 100);

                                        lock (lockObj)
                                        {
                                            if (matchPercentage > bestMatchPercentage)
                                            {
                                                bestMatch = item;
                                                bestMatchPercentage = matchPercentage;
                                            }
                                        }

                                        int lcsLength = LongestCommonSubsequence.Compute(text, pattern2);
                                        similarity = (double)lcsLength / Math.Min(text.Length, pattern2.Length);
                                        matchPercentage = (int)(similarity * 100);

                                        lock (lockObj)
                                        {
                                            if (matchPercentage > bestMatchPercentage)
                                            {
                                                bestMatchlcs = item;
                                                bestMatchPercentagelcs = matchPercentage;
                                            }
                                        }
                                    } else {
                                        int lcsLength = LongestCommonSubsequence.Compute(text, pattern2);
                                        double similarity = (double)lcsLength / Math.Min(text.Length, pattern2.Length);
                                        int matchPercentage = (int)(similarity * 100);

                                        lock (lockObj)
                                        {
                                            if (matchPercentage > bestMatchPercentage)
                                            {
                                                bestMatchlcs = item;
                                                bestMatchPercentagelcs = matchPercentage;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }), parallelOptions.CancellationToken);

                   lock (lockObj)
                    {
                        if (bestMatchPercentage < 40 && bestMatchPercentagelcs > bestMatchPercentage)
                        {
                            bestMatch = bestMatchlcs;
                            bestMatchPercentage = bestMatchPercentagelcs;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Full image match found and search canceled.");
                }
            }

            if (bestMatch != null)
            {
                _logger.LogInformation($"Found match with {bestMatchPercentage}% similarity");
                return Ok(new SearchResult { SidikJari = bestMatch, MatchPercentage = bestMatchPercentage });
            }

            return Ok(new { message = "No matching data found" });
        }
    }
}
