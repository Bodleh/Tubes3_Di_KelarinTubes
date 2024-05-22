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

        private string ConvertToBinaryString(byte[] data)
        {
            return string.Join(string.Empty, data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        private string ConvertBinaryStringToAscii(string binaryString)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < binaryString.Length; i += 8)
            {
                string byteString = binaryString.Substring(i, 8);
                sb.Append((char)Convert.ToByte(byteString, 2));
            }

            return sb.ToString();
        }
        private string FindBestPixelSegment(byte[] imagePixels, int pixelCount)
        {
            int totalPixels = imagePixels.Length;
            int middleIndex = totalPixels / 2;
            int startIndex = Math.Max(0, middleIndex - pixelCount / 2);
            int endIndex = Math.Min(totalPixels, startIndex + pixelCount);

            // extract
            byte[] segment = imagePixels.Skip(startIndex).Take(endIndex - startIndex).ToArray();

            // convert each pixel to 8-bit binary string
            StringBuilder binaryStringBuilder = new StringBuilder();
            foreach (var pixel in segment)
            {
                binaryStringBuilder.Append(Convert.ToString(pixel, 2).PadLeft(8, '0'));
            }

            return binaryStringBuilder.ToString();
        }

        private byte[] ConvertImageToGrayscaleByteArray(byte[] imageData)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imageData))
            {
                image.Mutate(x => x.Grayscale());
                return ConvertImageToByteArray(image);
            }
        }

        private byte[] ConvertImageToByteArray(Image<Rgba32> image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.SaveAsBmp(ms);
                return ms.ToArray();
            }
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
                fileData = ConvertImageToGrayscaleByteArray(original);
                _logger.LogInformation("originalData: " + original.Length);
                _logger.LogInformation("grayData: " + fileData.Length);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid base64 data" });
            }
            var bestSegment = FindBestPixelSegment(fileData, 200);
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
            HashSet<SidikJari> SetFound = new HashSet<SidikJari>();
            // SidikJari? closest = null;
            // int smallest = int.MaxValue;

            foreach (var item in sidikJariList)
            {
                if (!string.IsNullOrEmpty(item.BerkasCitra))
                {
                    var filePath = item.BerkasCitra;
                    var realpath = "../" + "../" + filePath;

                    if (System.IO.File.Exists(realpath))
                    {
                        byte[] originalBytes = await System.IO.File.ReadAllBytesAsync(realpath);
                        byte[] grayBytes = ConvertImageToGrayscaleByteArray(originalBytes);
                        string itemBinary = ConvertToBinaryString(grayBytes);
                        string text = ConvertBinaryStringToAscii(itemBinary);
                        string pattern = ConvertBinaryStringToAscii(bestSegment);

                        if (!SetFound.Contains(item) && BoyerMoore.BMSearch(pattern, text)) {
                            SetFound.Add(item);
                        }
                        // else {
                        //     int totalDistance = LevenshteinDistance.Compute(text, pattern);
                        //     if (totalDistance < smallest) {
                        //         smallest = totalDistance;
                        //         closest = item;
                        //     }
                        // }
                    }
                    else
                    {
                        item.BerkasCitra = null;
                    }
                }
            }
            // if (SetFound.Count == 0 && closest != null) {
            //     SetFound.Add(closest);
            // }
            List<SidikJari> ListFound = SetFound.ToList();
            _logger.LogInformation("Found data length: " + ListFound.Count);
            return Ok(new SearchResult { SidikJari = ListFound });
        }
    }
}
