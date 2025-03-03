using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Vote: IData
    {
        [JsonInclude]
        public string UserId { get; set; } = "";

        [JsonInclude]
        public string Candidate { get; set; } = "";

        [JsonConstructor]
        public Vote(string userId, string candidate)
        {
            UserId = userId;
            Candidate = candidate;
        }

        public object Clone() => new Vote(UserId, Candidate);

    }
}
