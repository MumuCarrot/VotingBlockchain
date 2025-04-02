#define DEBUG

using System.Text;
using System.Security.Cryptography;
using VotingBlockchain.Abstract;
using VotingBlockchain.Datatypes;
using System.Text.Json;

namespace VotingBlockchain.Services
{
    public class Miner : AService
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string HostName = @"http://localhost:5000";

        public async Task<Block?> TryGetData() 
        {
            string url = "http://localhost:5000/trygetdata";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<Block>(json);
                return result;
            }
            catch { }
            return null;
        }

        public async Task AddBlock(Block block)
        {
            Dictionary<string, string> data = new()
            {
                { "block", JsonSerializer.Serialize(block) }
            };
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            string url = "http://localhost:5000/addblock";
            HttpResponseMessage response = await client.PostAsync(url, content);
            string json = await response.Content.ReadAsStringAsync();
        }

        public override void Start() 
        {
            Console.WriteLine("Miner is running on " + HostName);
            if (serviceTask == null || serviceTask.IsCompleted)
            {
                isRunning = true;
                serviceTask = new Task(async () =>
                {
                    while (isRunning)
                    {
                        try
                        {
                            var data = await TryGetData();

                            if (data is not null)
                            {
                                data = SearchHash(data);

                                await AddBlock(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Mining error: {ex.Message}");
                        }

                        await Task.Delay(5000);
                    }
                });
                serviceTask.Start();
            }
        }

        public override void Stop() 
        {
            isRunning = false;
            serviceTask?.Wait();
        }

        public Block SearchHash(Block block)
        {
            string hash = new string('1', block.Difficulty);
            while (hash[..block.Difficulty] != new string('0', block.Difficulty))
            {
                string blockData = block.Index + block.ElectionId + block.Timestamp.ToString() + block.Nonce + block.Difficulty + block.PreviousHash + block.EncryptedData + block.PublicData;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                    hash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
                block.Nonce++;
            }
            block.Nonce--;
            block.ThisHash = hash;
#if DEBUG
            Console.WriteLine(hash);
#endif
            return block;
        }
    }
}
