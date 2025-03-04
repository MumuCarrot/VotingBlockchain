using VotingBlockchain.Interfaces;

namespace VotingBlockchain.Abstract
{
    public abstract class AData : IData
    {
        public string EncryptedData { get; set; } = "";

        public object Clone() => new { EncryptedData = EncryptedData };
    }
}
