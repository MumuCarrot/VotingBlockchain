using System.Text.Json;
using System.Text.RegularExpressions;

namespace VotingBlockchain
{
    public partial class UserDatabase
    {
        private readonly List<User> users = new List<User>();
        public List<User> UserList { get; } = [];

        private const string FilePath = "users.bin";

        [GeneratedRegex("^(?:[А-ЩЬЮЯҐЄІЇ]{2}\\d{6}|\\d{9})$")]
        private static partial Regex UkrainianStandartPasspordRegex();

        public UserDatabase()
        {
            LoadFromFile();
        }

        public bool Register(string userId, string password)
        {
            if (!UkrainianStandartPasspordRegex().IsMatch(userId))
                return false;

            if (users.Any(u => u.Id == userId))
                return false;

            users.Add(User.NewUser(userId, password));
            SaveToFile();
            return true;
        }

        public User? Authenticate(string userId, string password)
        {
            return users.FirstOrDefault(u => u.Id == userId && u.ValidatePassword(password));
        }

        public void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(users, options);
            FileEncryption.EncryptToFile(FilePath, json);
        }

        private void LoadFromFile()
        {
            string decryptedJson = FileEncryption.DecryptFromFile(FilePath);
            if (!string.IsNullOrEmpty(decryptedJson))
            {
                var loadedUsers = JsonSerializer.Deserialize<List<User>>(decryptedJson);
                if (loadedUsers != null)
                    users.AddRange(loadedUsers);
            }
        }
    }
}
