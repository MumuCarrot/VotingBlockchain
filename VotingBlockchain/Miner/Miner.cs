﻿#define DEBUG

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VotingBlockchain.Datatypes.Abstract;
using VotingBlockchain.Datatypes.Classes;
using VotingBlockchain.Mempool;

namespace VotingBlockchain.Services
{
    public class Miner : AService
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string HostName = @"http://localhost:5000";

        private static string NowStr => "[" + DateTime.UtcNow + "] | ";

        public async Task<MempoolItem?> TryGetData()
        {
            string url = "http://localhost:5000/trygetdata";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<MempoolItem>(json);
                return result;
            }
            catch
            {
                Console.WriteLine(NowStr + "No data in mempool on paired node");
            }
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
            Console.WriteLine(NowStr + "Block pushed to node");
        }

        public override void Start()
        {
            Console.WriteLine(NowStr + "Miner paired with node on " + HostName);
            if (serviceTask == null || serviceTask.IsCompleted)
            {
                isRunning = true;
                serviceTask = new Task(async () =>
                {
                    while (isRunning)
                    {
                        await Task.Delay(5000);

                        try
                        {
                            var data = await TryGetData();

                            if (data is null) continue;

                            if (data.preInfo is null) continue;

                            await AddBlock(SearchHash(data.preInfo));
                            
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Mining error: {ex.Message}");
                        }
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
