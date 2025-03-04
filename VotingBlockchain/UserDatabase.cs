#define DEBUG

using Npgsql;
using System.Text.RegularExpressions;

namespace VotingBlockchain
{
    public partial class UserDatabase
    {
        private readonly DBAdapter _adapter;

        [GeneratedRegex(@"^(?:[А-ЩЬЮЯҐЄІЇ]{2}\d{6}|\d{9})$")]
        private static partial Regex UkrainianStandartPasspordRegex();

        public UserDatabase(DBAdapter adapter) 
        {
            _adapter = adapter;
        }

        public async Task<List<string>?> RegisterAsync(string username, string password)
        {
            if (!UkrainianStandartPasspordRegex().IsMatch(username))
                return null;

            var temp = User.NewUser(username, password);
            string query = "INSERT INTO users (username, password, publicKey) VALUES (@username, @password, @publicKey)";
            var parameters = new Dictionary<string, object>
            {
                { "username", temp.Username },
                { "password", temp.PasswordHash },
                { "publicKey", temp.PublicKey }
            };
            await _adapter.ExecuteNonQueryAsync(query, parameters);
            
            return [temp.PublicKey, temp.PrivateKey];
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            string query = "SELECT * FROM users WHERE username = @username AND password = @password";
            User temp = new User(username, password);
            var parameters = new Dictionary<string, object>
            {
                { "username", temp.Username },
                { "password", temp.PasswordHash }
            };

            var users = await _adapter.ExecuteQueryAsync(query, parameters);
            if (!(users.Count > 0)) return null;

            temp.Id = users[0]["id"].ToString()!;
            temp.Username = users[0]["username"].ToString()!;
            temp.PasswordHash = users[0]["password"].ToString()!;
            temp.PublicKey = users[0]["publickey"].ToString()!;
            temp.Role = int.Parse(users[0]["role"].ToString()!);

            return temp;
        }

        public async Task<bool> UserExistAsync(string username)
        {
            string query = "SELECT COUNT(*) FROM users WHERE username = @username";
            var parameters = new Dictionary<string, object>
            {
                { "username", username }
            };
            var result = await _adapter.ExecuteScalarAsync(query, parameters);

            return Convert.ToInt32(result) > 0;
        }
    }
}
