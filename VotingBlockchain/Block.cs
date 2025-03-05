using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VotingBlockchain
{
    public class Block
    {
        public int Index { get; set; } = -1;

        public int ElectionId { get; set; } = -1;

        public long Timestamp { get; set; } = 0;

        public string PreviousHash { get; set; } = "";

        public string ThisHash { get; set; } = "";

        public int Nonce { get; set; } = 0;

        public int Difficulty { get; set; } = 3;

        public string EncryptedData { get; set; } = "";

        public Block(int index, int electionId, long timestamp, string previousHash, string thisHash, int nonce, int difficulty, string encryptedData) 
        {
            Index = index;
            ElectionId = electionId;
            Timestamp = timestamp;
            PreviousHash = previousHash;
            ThisHash = thisHash;
            Nonce = nonce;
            Difficulty = difficulty;
            EncryptedData = encryptedData;
        }

        public Block(int index, int electionId, string previousHash, string user, string option, string publicKey)
            : this(user, option, publicKey)
        {
            Index = index;
            PreviousHash = previousHash;
            ElectionId = electionId;
        }

        public Block(string user, string option, string publicKey)
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            EncryptedData = EncryptVote(user, publicKey) + "_" + option;
        }

        protected static string EncryptVote(string data, string publicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.KeySize = 2048;
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
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

        public static Block CreateGenesisBlock(int electionId)
        {
            var b = new Block(0, electionId, "0", "0", "0", "0");
            b.ThisHash = b.CalculateHash();
            return b;
        }

        public string CalculateHash()
        {
            string blockData = Index + ElectionId + Timestamp.ToString() + Nonce + Difficulty + PreviousHash + EncryptedData;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
