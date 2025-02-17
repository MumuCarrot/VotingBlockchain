using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace VotingBlockchain
{
    public class User
    {
        // User passport data
        [JsonInclude]
        public string Id { get; private set; } = "";

        [JsonInclude]
        public string PasswordHash { get; set; } = "";

        [JsonInclude]
        public string PublicKey { get; private set; } = "";

        private string PrivateKey { get; set; } = "";

        [JsonConstructor]
        public User(string id, string passwordHash, string publicKey)
            : this(id, passwordHash)
        {
            PasswordHash = passwordHash;
            PublicKey = publicKey;
            PrivateKey = "";
        }

        public User(string userId, string password)
        {
            Id = userId;
            if (PasswordHash.Length == 0)
                PasswordHash = HashPassword(password);
            GenerateKeys(PublicKey == "");
        }

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private void GenerateKeys(bool withPublic)
        {
            using RSA rsa = RSA.Create();
            if (withPublic)
                PublicKey = rsa.ToXmlString(false);
            PrivateKey = rsa.ToXmlString(true);
        }

        public bool ValidatePassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        public string? TryDecryptVote(string encryptedVote)
        {
            return Vote.TryDecryptVote(encryptedVote, PrivateKey);
        }
    }
}
