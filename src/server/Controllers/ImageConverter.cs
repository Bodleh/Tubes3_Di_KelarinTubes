using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace server
{
    public static class ImageConverter
    {
        public static string ConvertByteToAsciiString(byte[] data)
        {
            string binaryString = string.Concat(data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            return ConvertBinaryStringToAscii(binaryString);
        }

        public static string ConvertBinaryStringToAscii(string binaryString)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < binaryString.Length; i += 8)
            {
                sb.Append((char)Convert.ToByte(binaryString.Substring(i, 8), 2));
            }

            return sb.ToString();
        }

        public static string FindBestPixelSegment(byte[] imagePixels, int pixelCount)
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

            return ConvertByteToAsciiString(imagePixels.Skip(bestStartIndex).Take(pixelCount).ToArray());
        }

        public static byte[] ConvertImageToGrayscaleByteArray(byte[] imageData)
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
    }
}