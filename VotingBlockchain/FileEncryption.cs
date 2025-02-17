using System.Security.Cryptography;
using System.Text;

namespace VotingBlockchain
{
    public class FileEncryption
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("EB8ZEzmZmh/zCKWvnRPvAquvu40xFFZL");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("LWaz66lXLfZ1F6CR");

        public static void EncryptToFile(string filePath, string data)
        {
            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using FileStream fileStream = new(filePath, FileMode.Create);
            using CryptoStream cryptoStream = new(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using StreamWriter writer = new(cryptoStream);
            writer.Write(data);
        }

        public static string DecryptFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using FileStream fileStream = new(filePath, FileMode.Open);
            using CryptoStream cryptoStream = new(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader reader = new(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}
