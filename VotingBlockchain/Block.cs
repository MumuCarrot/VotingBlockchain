using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using VotingBlockchain.Interfaces;
using System.Security.Cryptography;
using System.Xml.Linq;
using System;

namespace VotingBlockchain
{
    public class Block : IBlock
    {
        [JsonInclude]
        public int Index { get; set; } = -1;

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
        public Vote? Vote { get; set; } = null;

        [JsonConstructor]
        public Block(int index, string previousHash, Vote? vote, string thisHash, long timestamp)
            : this(index, previousHash)
        {
            ThisHash = thisHash;
            Timestamp = timestamp;
            Vote = vote;
        }

        public Block(int index, string previousHash)
        {
            Index = index;
            if (Timestamp == 0)
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PreviousHash = previousHash;
        }

        public static Block CreateGenesisBlock() => new Block(0, "0");

        public string CalculateHash()
        {
            string blockData = Index + Timestamp.ToString() + Nonce + Difficulty + PreviousHash + JsonSerializer.Serialize(Vote);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public object Clone() => new Block(Index, PreviousHash, Vote, ThisHash, Timestamp);
    }
}
