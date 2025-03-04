namespace VotingBlockchain
{
    public class Election
    {
        public int Index { get; set; } = -1;

        public string Name { get; set; } = "";
        
        public long StartDate { get; set; } = 0;

        public long EndDate { get; set; } = 0;
    }
}
