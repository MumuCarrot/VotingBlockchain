using System.Net;
using System.Text;
using System.Text.Json;
using VotingBlockchain.Abstract;
using VotingBlockchain.Datatypes;

namespace VotingBlockchain
{
    public partial class FullNode
    {
        private string hostName = @"http://localhost:5000/";

        private Output _output;
        private List<AService> _services = [];

        private Mempool Mempool { get; }
        private Blockchain BlockChain { get; }

        public FullNode()
        {
            BlockChain = new Blockchain(this);
            Mempool = new Mempool();

            _output = new Output(Console.WriteLine);
        }

        public void SetOutput(Action<string> action)
        {
            _output = new Output(action);
        }

        public void Output(string message)
        {
            _output.Write(message);
        }

        public async Task HostServer() 
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(hostName);
            listener.Start();
            Console.WriteLine("Node is running on " + hostName);

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/register")
                {
                    await HandleRegister(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/login")
                {
                    await HandleLogin(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/uexists")
                {
                    await HandleUserExists(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/election")
                {
                    await HandleElection(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/elections")
                {
                    await HandleElections(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/cuelections")
                {
                    await HandleCurrentElections(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/coelections")
                {
                    await HandleCompletedElections(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/options")
                {
                    await HandleOptions(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/trygetdata")
                {
                    HandleTryGetData(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/canivotehere")
                {
                    await HandleCanIVoteHere(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/getvotedelections")
                {
                    await HandleGetVotedElections(request, response);
                }
                else if (request.HttpMethod == "POST" && request.Url?.AbsolutePath == "/addblock")
                {
                    await HandleAddBlock(request, response);
                }
                else if (request.HttpMethod == "POST" && request.Url?.AbsolutePath == "/vote")
                {
                    await HandleVote(request, response);
                }
                else if (request.HttpMethod == "POST" && request.Url?.AbsolutePath == "/uvote")
                {
                    await HandleUserVote(request, response);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                response.Close();
            }
        }

        private static void SendResponse(HttpListenerResponse response, object data, HttpStatusCode statusCode)
        {
            string jsonResponse = JsonSerializer.Serialize(data);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

            response.StatusCode = (int)statusCode;
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";

            using var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }

        private static async Task<Dictionary<string, string>> GetRequestParameters(HttpListenerRequest request)
        {
            var parameters = new Dictionary<string, string>();

            foreach (string key in request.QueryString.Keys)
                if (key != null) parameters[key] = request.QueryString[key] ?? "";

            if (request.HttpMethod == "POST" && request.HasEntityBody)
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string json = await reader.ReadToEndAsync();

                try
                {
                    var bodyParams = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (bodyParams != null)
                    {
                        foreach (var param in bodyParams)
                            parameters[param.Key] = param.Value;
                    }
                }
                catch (JsonException)
                {
                    Console.WriteLine("Error parsing JSON from request body.");
                }
            }

            return parameters;
        }

        private async Task HandleUserExists(HttpListenerRequest request, HttpListenerResponse response)
        {
            var userData = await GetRequestParameters(request);
            if (userData == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var result = await Blockchain.DBQuery.UserExistAsync(userData["username"]);
            if (result)
                SendResponse(response, result, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Conflict);
        }

        private static async Task HandleRegister(HttpListenerRequest request, HttpListenerResponse response)
        {
            var userData = await GetRequestParameters(request);
            if (userData == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var keys = await Blockchain.DBQuery.RegisterAsync(userData["username"], userData["password"]) ?? throw new Exception("Keys is null");
            if (keys is not null)
                SendResponse(response, keys, HttpStatusCode.OK);
            else
                SendResponse(response, "User already exists", HttpStatusCode.Conflict);
        }

        private static async Task HandleLogin(HttpListenerRequest request, HttpListenerResponse response)
        {
            var userData = await GetRequestParameters(request);
            if (userData == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }
            
            User? user = await Blockchain.DBQuery.AuthenticateAsync(userData["username"], userData["password"]);
            if (user is not null)
                SendResponse(response, user, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleElection(HttpListenerRequest request, HttpListenerResponse response)
        {
            var userData = await GetRequestParameters(request);
            if (userData == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            Election? election = await Blockchain.DBQuery.GetElectionAsync(int.Parse(userData["electionid"]));
            if (election is not null)
                SendResponse(response, election, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleElections(HttpListenerRequest request, HttpListenerResponse response)
        {
            var elections = await Blockchain.DBQuery.GetElectionsAsync();
            if (elections is not null)
                SendResponse(response, elections, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleCurrentElections(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var elections = await Blockchain.DBQuery.GetCurrentElectionsAsync(long.Parse(data["unix"]));
            if (elections is not null)
                SendResponse(response, elections, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleCompletedElections(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var elections = await Blockchain.DBQuery.GetCompletedElectionsAsync(long.Parse(data["unix"]));
            if (elections is not null)
                SendResponse(response, elections, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleOptions(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var options = await Blockchain.DBQuery.GetOptionsAsync(int.Parse(data["electionid"]));
            if (options is not null)
                SendResponse(response, options, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private async Task HandleVote(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            try
            {
                await BlockChain.VoteAsync(int.Parse(data["electionid"]), int.Parse(data["userid"]), data["username"], data["publickey"], int.Parse(data["option"]));
                SendResponse(response, "OK", HttpStatusCode.OK);
            }
            catch
            { 
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
            }
        }

        private async Task HandleUserVote(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var option = await BlockChain.GetUserVoteAsync(data["username"], int.Parse(data["electionid"]), data["privatekey"]);
            if (option is not null)
                SendResponse(response, option, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private void HandleTryGetData(HttpListenerRequest request, HttpListenerResponse response)
        {
            var block = Mempool.GetInputData();
            if (block is not null)
                SendResponse(response, block, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private async Task HandleAddBlock(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            Block? block = JsonSerializer.Deserialize<Block>(data["block"]);

            if (block is null) return;

            await BlockChain.AddBlockAsync(block);
            if (block is not null)
                SendResponse(response, "OK", HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid credentials", HttpStatusCode.Unauthorized);
        }

        private static async Task HandleCanIVoteHere(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var nCounter = await Blockchain.DBQuery.GetNCounterAsync(int.Parse(data["userid"]), int.Parse(data["electionid"]));

            if (nCounter is null) SendResponse(response, true, HttpStatusCode.OK);

            var revotes = await Blockchain.DBQuery.GetRevotesAsync(int.Parse(data["electionid"]));

            if (revotes is null) SendResponse(response, "Invalid credentials", HttpStatusCode.OK);

            if (nCounter <= revotes)
                SendResponse(response, true, HttpStatusCode.OK);
            else
                SendResponse(response, false, HttpStatusCode.Unauthorized);
        }

        private static async Task HandleGetVotedElections(HttpListenerRequest request, HttpListenerResponse response)
        {
            var data = await GetRequestParameters(request);
            if (data == null)
            {
                SendResponse(response, "Invalid data", HttpStatusCode.BadRequest);
                return;
            }

            var result = await Blockchain.DBQuery.GetVotedElectionsAsync(int.Parse(data["voterid"]));

            if (result is not null)
                SendResponse(response, result, HttpStatusCode.OK);
            else
                SendResponse(response, "Invalid data", HttpStatusCode.Unauthorized);
        }
    }
}
