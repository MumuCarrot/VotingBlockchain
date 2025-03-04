namespace VotingBlockchain
{
    /// <summary>
    /// Represents a vote.
    /// </summary>
    public class Vote
    {
        /// <summary>
        /// Gets or sets the data of the current voter.
        /// </summary>
        public string? EncryptedData { get; set; }

        /// <summary>
        /// Gets or sets remaining re-vote attempts for current voter.
        /// </summary>
        public int? RemainigRevoteAttempt { get; set; }
    }
}
