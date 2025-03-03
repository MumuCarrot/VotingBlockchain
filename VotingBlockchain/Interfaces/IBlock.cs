namespace VotingBlockchain.Interfaces
{
    public interface IBlock: ICloneable 
    {
        public int Index { get; set; }

        public long Timestamp { get; set; }

        public string PreviousHash { get; set; }

        public string ThisHash { get; set; }

        public int Nonce { get; set; }

        public int Difficulty { get; set; }

        public Vote? Vote { get; set; }
    }
}
