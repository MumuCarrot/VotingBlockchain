using System.Security.Cryptography;
using System.Text;

namespace VotingBlockchain.Datatypes
{
    public class User
    {

        public string Id { get; set; } = "";

        public string Username { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public string PublicKey { get; set; } = "";

        public string PrivateKey { get; set; } = "";
        
        public int Role { get; set; } = 0;

        public User(string Username, string PasswordHash)
        {
            this.Username = Username;
            this.PasswordHash = HashPassword(PasswordHash);
        }

        public void GenerateKeysForUser()
        {
            if (PublicKey.Length == 0 && PrivateKey.Length == 0) 
            { 
                using RSA rsa = RSA.Create();
                rsa.KeySize = 2048;
                PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            }
        }

        public static User NewUser(string username, string password) 
        { 
            var temp = new User(username, password);
            temp.GenerateKeysForUser();
            return temp;
        }

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
