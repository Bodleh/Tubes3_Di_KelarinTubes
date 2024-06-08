using System;
using System.Text;

namespace server
{
    public static class Encrypt
    {
        private static readonly byte[] key = Encoding.UTF8.GetBytes("TAMATTUBESSTIMA3");

        public static string GetEncrypt(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes = new byte[dataBytes.Length];

            for (int i = 0; i < dataBytes.Length; i++)
            {
                byte shiftedByte = (byte)((dataBytes[i] + 21) % 256);
                encryptedBytes[i] = (byte)(shiftedByte ^ key[i % key.Length]);
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static class Decrypt
    {
        private static readonly byte[] key = Encoding.UTF8.GetBytes("TAMATTUBESSTIMA3");

        public static string GetDecrypt(string encryptedData)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                byte xorByte = (byte)(encryptedBytes[i] ^ key[i % key.Length]);
                decryptedBytes[i] = (byte)((xorByte - 21 + 256) % 256);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}