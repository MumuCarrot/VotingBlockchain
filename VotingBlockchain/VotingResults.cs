namespace VotingBlockchain
{
    public class VotingResults
    {
        public Dictionary<string, int> Result { get; set; } = [];

        public VotingResults(List<Block> chain)
        {
            foreach (var block in chain) 
            {
                if (block.Vote?.Candidate is not null) 
                {
                    if (!Result.TryAdd(block.Vote.Candidate, 1))
                        Result[block.Vote.Candidate]++;
                }
            }
        }
    }
}
