using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using VotingBlockchain.Interfaces;
using System.Security.Cryptography;

namespace VotingBlockchain
{
    public class Block
    {
        [JsonInclude]
        public int Index { get; set; } = -1;

        [JsonInclude]
        public int ElectionIndex { get; set; } = -1;

        [JsonInclude]
        public long Timestamp { get; set; } = 0;

        [JsonInclude]
        public string PreviousHash { get; set; } = "";

        [JsonInclude]
        public string ThisHash { get; set; } = "";

        [JsonInclude]
        public int Nonce { get; set; } = 0;

        [JsonInclude]
        public int Difficulty { get; set; } = 3;

        [JsonInclude]
        public List<Vote>? Stamp { get; set; } = null;

        public Block(int index, string previousHash, string? jsonStamp)
        {
            Index = index;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PreviousHash = previousHash;
            if (jsonStamp is not null)
                try 
                { 
                    JsonSerializer.Deserialize<List<Vote>>(jsonStamp);
                }
                catch
                {
                    Console.WriteLine($"Election id: {ElectionIndex} | Stamp json may be empty when deserialization is attempted, allocate new memory...");
                    Stamp = [];
                }
            else Stamp = [];
        }

        public static Block CreateGenesisBlock() => new Block(0, "0", null);

        public string CalculateHash()
        {
            string blockData = Index + Timestamp.ToString() + Nonce + Difficulty + PreviousHash + JsonSerializer.Serialize(Stamp);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
