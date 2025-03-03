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

        [JsonConstructor]
        public User(string Id, string PasswordHash)
        {
            this.Id = Id;
            this.PasswordHash = PasswordHash;
        }

        public static User NewUser(string userId, string password) => new User(userId, HashPassword(password));

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool ValidatePassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }
    }
}
