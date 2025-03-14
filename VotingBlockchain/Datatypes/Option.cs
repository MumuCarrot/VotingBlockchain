namespace VotingBlockchain.Datatypes
{
    public class Option
    {
        public int Index { get; set; } = -1;

        public int ElectionId { get; set; } = -1;

        public string OptionText { get; set; } = "";
    }
}
