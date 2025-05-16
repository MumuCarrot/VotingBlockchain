namespace VotingBlockchain.Datatypes.Classes
{
    public class Election
    {
        public int Id { get; set; } = -1;

        public string Name { get; set; } = "";
        
        public long StartDate { get; set; } = 0;

        public long EndDate { get; set; } = 0;

        public string Description { get; set; } = "";
    }
}
