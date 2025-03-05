#define DEBUG

using System.Text;
using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Blockchain
    {
        private INode Node { get; }

        private DBAdapter _adapter;

        public Blockchain(DBAdapter adapter, INode node)
        {
            _adapter = adapter;
            Node = node;
        }

        public async Task<Block?> GetLatestBlockAsync(int electionId) 
        { 
            string query = "SELECT * FROM blockChainVotes WHERE electionId = @electionId ORDER BY id DESC LIMIT 1;";
            var parameters = new Dictionary<string, object>()
            {
                { "electionId", electionId }
            };
            
            var dict = await _adapter.ExecuteQueryAsync(query, parameters);
            if (dict is null || dict.Count == 0) return null;

            return new Block(
                (int)dict[0]["index"],
                (int)dict[0]["electionid"],
                (long)dict[0]["timestamp"],
                (string)dict[0]["previoushash"],
                (string)dict[0]["thishash"],
                (int)dict[0]["nonce"],
                (int)dict[0]["difficulty"],
                (string)dict[0]["encrypteddata"]
            );
        }

        public async Task<List<Block>?> GetBlockChainAsync(int electionId) 
        { 
            string query = "SELECT * FROM blockChainVotes WHERE electionId = @electionId";
            var parameters = new Dictionary<string, object>()
            {
                { "electionId", electionId }
            };

            var dict = await _adapter.ExecuteQueryAsync(query, parameters);
            if (dict is null || dict.Count == 0) return null;

            List<Block> temp = [];
            foreach (var d in dict) 
            {
                var b = new Block(
                    (int)d["index"],
                    (int)d["electionid"],
                    (long)d["timestamp"],
                    (string)d["previoushash"],
                    (string)d["thishash"],
                    (int)d["nonce"],
                    (int)d["difficulty"],
                    (string)d["encrypteddata"]
                );
                temp.Add(b);
            }
            return temp;
        }

        public async Task<bool> ValidateBlockAsync(Block? block)
        {
            if (block is null) return false;

#if DEBUG
            Console.WriteLine("after: is not null");
#endif

            var resp = await GetLatestBlockAsync(block.ElectionId);

            if (resp is null) return false;

#if DEBUG
            Console.WriteLine("previ: is not null" );
#endif

            if (resp.ThisHash != block.PreviousHash) return false;

#if DEBUG
            Console.WriteLine("Validation: Prev this hash fine");
#endif

            if (resp.Index + 1 != block.Index) return false;

#if DEBUG
            Console.WriteLine("Validation: Index fine");
#endif

            if (block.ThisHash != block.CalculateHash()) return false;

#if DEBUG
            Console.WriteLine("Validation: Calc hash fine");
#endif

            return true;
        }

        public async Task<bool> ValidateChainAsync(int electionId)
        {
            var blockChain = await GetBlockChainAsync(electionId);

            if (blockChain is null || blockChain.Count == 0) return false;

            if (blockChain.Count == 1) return true; // Only genesis block

            for (int i = 1; i < blockChain.Count; i++)
            {
                Block previousBlock = blockChain[i - 1];
                Block afterBlock = blockChain[i];

#if DEBUG
                Console.WriteLine("after: " + afterBlock.PreviousHash);
                Console.WriteLine("previ: " + previousBlock.ThisHash);
#endif
                if (previousBlock.Index + 1 != afterBlock.Index)
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Index fine");
#endif
                if (afterBlock.ThisHash != afterBlock.CalculateHash())
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Calc hash fine");
#endif
                if (afterBlock.PreviousHash != previousBlock.ThisHash)
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Prev this hash fine");
#endif
            }
            return true;
        }

        public async Task<bool> ValidateChainAsync(Block newBlock)
        {
            var blockChain = await GetBlockChainAsync(newBlock.ElectionId);

            if (blockChain is null || blockChain.Count == 0) return false;

            blockChain.Add(newBlock);
            for (int i = 1; i < blockChain.Count; i++)
            {
                Block previousBlock = blockChain[i - 1];
                Block afterBlock = blockChain[i];

#if DEBUG
                Console.WriteLine("after: " + afterBlock.PreviousHash);
                Console.WriteLine("previ: " + previousBlock.ThisHash);
#endif
                if (previousBlock.Index + 1 != afterBlock.Index)
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Index fine");
#endif
                if (afterBlock.ThisHash != afterBlock.CalculateHash())
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Calc hash fine");
#endif
                if (afterBlock.PreviousHash != previousBlock.ThisHash)
                    return false;
#if DEBUG
                Console.WriteLine("Validation: Prev this hash fine");
#endif
            }
            return true;
        }

        public async Task VoteAsync(int electionId, User user, int option)
        {
            var resp = await GetLatestBlockAsync(electionId);

            if (resp is null) return;

            Node.Mempool.AddToInputMempool(new Block(resp.Index + 1, resp.ElectionId, resp.ThisHash, user.Username, option.ToString(), user.PublicKey));
        }

        public async Task AddBlockAsync(Block block)
        {
            if (!await ValidateBlockAsync(block)) return;

            if (!Node.Mempool.RemoveFromInputMempool(block)) return;

            string query = "INSERT INTO blockChainVotes (electionId, index, timestamp, previousHash, thisHash, nonce, difficulty, encryptedData) " +
                           "VALUES (@electionId, @index, @timestamp, @previousHash, @thisHash, @nonce, @difficulty, @encryptedData)";
            var parameters = new Dictionary<string, object>()
            {
                { "electionId", block.ElectionId },
                { "index", block.Index },
                { "timestamp", block.Timestamp },
                { "previousHash", block.PreviousHash },
                { "thisHash", block.ThisHash },
                { "nonce", block.Nonce },
                { "difficulty", block.Difficulty },
                { "encryptedData", block.EncryptedData }
            };
            await _adapter.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task GetElectionResults(int electionId) 
        {
            var blockchain = await GetBlockChainAsync(electionId);
            var options = await Node.Mempool.GetOptionsAsync(electionId);
            var elections = await Node.Mempool.GetElectionsAsync();

            if (blockchain is null || blockchain.Count <= 1) 
            {
                Console.WriteLine("Election is empty.");
                return;
            }

            if (options is null || options.Count == 0)
            {
                Console.WriteLine("Can not upload options.");
                return;
            }

            if (elections is null || elections.Count == 0)
            {
                Console.WriteLine("Can not upload elections.");
                return;
            }

            // Key - Username | Value - Option
            Dictionary<string, string> uniqueResults = new Dictionary<string, string>();

            foreach (var result in blockchain) 
            {
                int lastUnderscore = result.EncryptedData.LastIndexOf("_");

                if (lastUnderscore == -1) 
                { 
                    Console.WriteLine("Encrypted result uderscore not found.");
                }

                string before = result.EncryptedData.Substring(0, lastUnderscore);
                string after = result.EncryptedData.Substring(lastUnderscore + 1);

                if (after == "0") continue; // Skip genesis block

                if (uniqueResults.ContainsKey(before))
                {
                    uniqueResults[before] = after;
                }
                else
                {
                    uniqueResults.Add(before, after);
                }
            }

            Dictionary<string, int> resultsCounter = new Dictionary<string, int>();

            foreach (var result in uniqueResults) 
            {
                if (resultsCounter.ContainsKey(result.Value))
                    resultsCounter[result.Value]++;
                else
                    resultsCounter.Add(result.Value, 1);
            }

            try
            {
                using FileStream fs = new FileStream($"results_id{electionId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.txt", FileMode.Create);
                string? electionName = elections.Find(e => e.Id == electionId)?.Name;
                electionName ??= "Unknown election";

                var title = $"Results for {electionName}\n";
                Console.Write(title);
                fs.Write(Encoding.UTF8.GetBytes(title));
                for (var i = 0; i < options.Count; i++)
                {
                    var line = $"{options[i].OptionText} - {(resultsCounter.ContainsKey((i + 1).ToString()) ? resultsCounter[(i + 1).ToString()] : "0")}\n";
                    Console.Write(line);
                    fs.Write(Encoding.UTF8.GetBytes(line));
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
