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
    public class KMPController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<KMPController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public KMPController(DatabaseContext context, ILogger<KMPController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private string ConvertToBinaryString(byte[] data)
        {
            return string.Concat(data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        private string ConvertBinaryStringToAscii(string binaryString)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < binaryString.Length; i += 8)
            {
                sb.Append((char)Convert.ToByte(binaryString.Substring(i, 8), 2));
            }

            return sb.ToString();
        }

        private string FindBestPixelSegment(byte[] imagePixels, int pixelCount)
        {
            int totalPixels = imagePixels.Length;
            int bestStartIndex = 0;
            int bestIntensityVariation = int.MinValue;

            for (int i = 0; i <= totalPixels - pixelCount; i++)
            {
                int intensityVariation = 0;
                for (int j = i; j < i + pixelCount - 1; j++)
                {
                    intensityVariation += Math.Abs(imagePixels[j] - imagePixels[j + 1]);
                }

                if (intensityVariation > bestIntensityVariation)
                {
                    bestIntensityVariation = intensityVariation;
                    bestStartIndex = i;
                }
            }

            return ConvertToBinaryString(imagePixels.Skip(bestStartIndex).Take(pixelCount).ToArray());
        }

        private byte[] ConvertImageToGrayscaleByteArray(byte[] imageData)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imageData))
            {
                image.Mutate(x => x.Resize(image.Width / 4, image.Height / 4).Grayscale());
                using (MemoryStream ms = new MemoryStream())
                {
                    image.SaveAsBmp(ms);
                    return ms.ToArray();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SearchRequest request)
        {
            if (request == null || request.Data == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            _logger.LogInformation("Received a POST request to KMP endpoint");

            byte[] fileData;
            try
            {
                byte[] original = Convert.FromBase64String(request.Data);
                fileData = ConvertImageToGrayscaleByteArray(original);
                _logger.LogInformation("originalData: " + original.Length);
                _logger.LogInformation("grayData: " + fileData.Length);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid base64 data" });
            }

            var bestSegment = FindBestPixelSegment(fileData, 80);
            _logger.LogInformation("Segment data length: " + bestSegment.Length);

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
                        byte[] grayBytes = ConvertImageToGrayscaleByteArray(originalBytes);
                        string itemBinary = ConvertToBinaryString(grayBytes);
                        string text = ConvertBinaryStringToAscii(itemBinary);
                        string pattern = ConvertBinaryStringToAscii(bestSegment);

                        if (KnuthMorrisPratt.KMPSearch(text, pattern))
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
