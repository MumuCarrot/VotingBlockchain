namespace VotingBlockchain
{
    public class VotingResults
    {
        public Dictionary<string, int> Result { get; set; } = [];

        public VotingResults(List<Block> chain)
        {
            foreach (var block in chain) 
            {
                //if (block.Vote?.EncryptedData is not null) 
                //{
                //    if (!Result.TryAdd(block.Vote.EncryptedData, 1))
                //        Result[block.Vote.EncryptedData]++;
                //}
            }
        }
    }
}
