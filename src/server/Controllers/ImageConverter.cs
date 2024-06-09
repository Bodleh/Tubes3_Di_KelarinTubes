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

        public static string FindBestPixelSegment(byte[] imagePixels, int imageWidth, int imageHeight, int segmentWidth)
        {
            int bestStartIndex = 0;
            double bestRatioDifference = double.MaxValue;

            for (int row = 0; row < imageHeight; row++)
            {
                int rowStartIndex = row * imageWidth;
                for (int col = 0; col <= imageWidth - segmentWidth; col++)
                {
                    int blackCount = 0;
                    int whiteCount = 0;

                    for (int i = col; i < col + segmentWidth; i++)
                    {
                        if (imagePixels[rowStartIndex + i] == 0)
                        {
                            blackCount++;
                        }
                        else if (imagePixels[rowStartIndex + i] == 255)
                        {
                            whiteCount++;
                        }
                    }

                    double totalPixels = blackCount + whiteCount;
                    double ratioDifference = Math.Abs((blackCount / totalPixels) - 0.5);

                    if (ratioDifference < bestRatioDifference)
                    {
                        bestRatioDifference = ratioDifference;
                        bestStartIndex = rowStartIndex + col;
                    }
                }
            }

            return ConvertByteToAsciiString(imagePixels.Skip(bestStartIndex).Take(segmentWidth).ToArray());
        }

        public static (byte[], int, int) ConvertImageToGrayscaleByteArray(byte[] imageData, int scaleFactor = 1)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imageData))
            {
                int width = image.Width / scaleFactor;
                int height = image.Height / scaleFactor;

                image.Mutate(x => x.Resize(width, height).Grayscale());

                using (MemoryStream ms = new MemoryStream())
                {
                    image.SaveAsBmp(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var img = Image.Load<L8>(ms))
                    {
                        var pixelData = new byte[img.Width * img.Height];
                        img.CopyPixelDataTo(pixelData);
                        return (pixelData, img.Width, img.Height);
                    }
                }
            }
        }
    }
}