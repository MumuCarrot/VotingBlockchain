using VotingBlockchain.Datatypes.Interfaces;

namespace VotingBlockchain.Datatypes.Abstract
{
    public abstract class AData : IData
    {
        public string EncryptedData { get; set; } = "";

        public object Clone() => new { EncryptedData };
    }
}
