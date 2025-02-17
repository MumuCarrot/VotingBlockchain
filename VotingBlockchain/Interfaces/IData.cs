namespace VotingBlockchain.Interfaces
{
    public interface IData: ICloneable 
    {
        public string UserId { get; set; }

        public string EncryptedData { get; set; }
    }
}
