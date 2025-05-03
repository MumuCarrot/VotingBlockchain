using System.Text;
using System.Text.Json;
using VotingBlockchain.Datatypes;

namespace VotingBlockchain
{
    public static partial class Client
    {
        private static readonly HttpClient client = new HttpClient();

        public static User? User { get; set; }

        public static async Task<bool> UserExists(string username)
        {
            string url = $"http://localhost:5000/uexists?username={username}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<bool>(json);
            return result;
        }

        public static async Task<string[]?> Register(string username, string password)
        {
            string url = $"http://localhost:5000/register?username={username}&password={password}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<string[]>(json);
            return result;
        }

        public static async Task<User?> Login(string username, string password)
        {
            string url = $"http://localhost:5000/login?username={username}&password={password}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<User?>(json);
            return result;
        }

        public static async Task<Election?> GetElection(int electionId)
        {
            string url = $"http://localhost:5000/election?electionid={electionId}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Election?>(json);
            return result;
        }

        public static async Task<List<Election>?> GetElections()
        {
            string url = $"http://localhost:5000/elections";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Election>?>(json);
            return result;
        }

        public static async Task<List<Election>?> GetCurrentElections(long unix)
        {
            string url = $"http://localhost:5000/cuelections?unix={unix}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<List<Election>?>(json);
                return result;
            }
            catch (Exception ex) 
            {
                throw new Exception(json + Environment.NewLine + ex.Message);
            }
        }

        public static async Task<List<Election>?> GetCompletedElections(long unix)
        {
            string url = $"http://localhost:5000/coelections?unix={unix}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Election>?>(json);
            return result;
        }

        public static async Task<List<Option>?> GetOptions(int electionId)
        {
            string url = $"http://localhost:5000/options?electionid={electionId}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Option>?>(json);
            return result;
        }

        public static async Task Vote(int electionId, int userId, string username, string publicKey, int option)
        {
            Dictionary<string, string> data = new()
            {
                { "electionid", electionId.ToString() },
                { "userid", userId.ToString() },
                { "username", username },
                { "publickey", publicKey },
                { "option", option.ToString() }
            };
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            string url = $"http://localhost:5000/vote";
            HttpResponseMessage response = await client.PostAsync(url, content);
            string json = await response.Content.ReadAsStringAsync();
            return;
        }

        public static async Task<Option?> GetUserVote(string username, int electionId, string privateKey)
        {
            Dictionary<string, string> data = new()
            {
                { "electionid", electionId.ToString() },
                { "username", username },
                { "privatekey", privateKey }
            };
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            string url = $"http://localhost:5000/uvote";
            HttpResponseMessage response = await client.PostAsync(url, content);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Option?>(json);
            return result;
        }

        public static async Task<bool> CanIVoteHere(int electionId, int userid)
        {
            string url = $"http://localhost:5000/canivotehere?electionid={electionId}&userid={userid}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<bool>(json);
            return result;
        }

        public static async Task<List<Election>?> GetVotedElections(int userId)
        {
            string url = $"http://localhost:5000/getvotedelections?voterid={userId}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Election>>(json);
            return result;
        }

        public static async Task<List<Block>?> GetBlocks(int electionId)
        {
            string url = $"http://localhost:5000/blocks?electionid={electionId}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Block>>(json);
            return result;
        }

        public static async Task PostElection(string electionname, long startdate, long enddate, string description, int revote, string options)
        {
            Dictionary<string, string> data = new()
            {
                { "electionname", electionname },
                { "startdate", startdate.ToString() },
                { "enddate", enddate.ToString() },
                { "description", description },
                { "revote", revote.ToString() },
                { "options", options }
            };
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            string url = $"http://localhost:5000/postelection";
            HttpResponseMessage response = await client.PostAsync(url, content);
            string json = await response.Content.ReadAsStringAsync();
            return;
        }
    }
}
