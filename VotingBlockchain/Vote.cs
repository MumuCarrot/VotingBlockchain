using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Vote: IData
    {
        [JsonInclude]
        public string UserId { get; set; } = "";

        [JsonInclude]
        public string EncryptedData { get; set; } = "";

        [JsonConstructor]
        public Vote(string userId, string encryptedData)
        {
            UserId = userId;
            EncryptedData = encryptedData;
        }

        public Vote(string userId, string candidate, string publicKey)
        {
            UserId = userId;
            EncryptedData = EncryptVote(candidate, publicKey);
        }

        protected static string EncryptVote(string data, string publicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.FromXmlString(publicKey);
            byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedData);
        }

        public static string? TryDecryptVote(string encryptedData, string privateKey)
        {
            try
            {
                using RSA rsa = RSA.Create();
                rsa.FromXmlString(privateKey);
                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }

        public object Clone() => new Vote(UserId, EncryptedData);

    }
}
