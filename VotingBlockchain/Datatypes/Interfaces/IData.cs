namespace VotingBlockchain.Datatypes.Interfaces
{
    public interface IData: ICloneable
    {
        public string EncryptedData { get; set; }
    }
}
