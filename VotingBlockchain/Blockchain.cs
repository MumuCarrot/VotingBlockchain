//#define DEBUG

using System.Text;
using VotingBlockchain.Abstract;

namespace VotingBlockchain
{
    public class Blockchain
    {
        private ANode Node { get; }

        private DBAdapter _adapter;

        public Blockchain(DBAdapter adapter, ANode node)
        {
            _adapter = adapter;
            Node = node;
        }

        public async Task<Block?> GetLatestBlockAsync(int electionId) 
        { 
            string query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id ORDER BY id DESC LIMIT 1;";
            var parameters = new Dictionary<string, object>()
            {
                { "election_id", electionId }
            };
            
            var dict = await _adapter.ExecuteQueryAsync(query, parameters);
            if (dict is null || dict.Count == 0) return null;

            return new Block(
                (int)dict[0]["index"],
                (int)dict[0]["election_id"],
                (long)dict[0]["timestamp"],
                (string)dict[0]["previous_hash"],
                (string)dict[0]["this_hash"],
                (int)dict[0]["nonce"],
                (int)dict[0]["difficulty"],
                (string)dict[0]["encrypted_data"],
                (string)dict[0]["public_data"]
            );
        }

        public async Task<List<Block>?> GetBlockChainAsync(int electionId, bool isDesc = false) 
        {
            string query = "";
            if (!isDesc)
                query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id;";
            else
                query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id ORDER BY id DESC;";
            var parameters = new Dictionary<string, object>()
            {
                { "election_id", electionId }
            };

            var dict = await _adapter.ExecuteQueryAsync(query, parameters);
            if (dict is null || dict.Count == 0) return null;

            List<Block> temp = [];
            foreach (var d in dict) 
            {
                var b = new Block(
                    (int)d["index"],
                    (int)d["election_id"],
                    (long)d["timestamp"],
                    (string)d["previous_hash"],
                    (string)d["this_hash"],
                    (int)d["nonce"],
                    (int)d["difficulty"],
                    (string)d["encrypted_data"],
                    (string)d["public_data"]
                );
                temp.Add(b);
            }
            return temp;
        }

        public async Task<Option?> GetUserVoteAsync(User user, int electionId, string privateKey)
        {
            var blockchain = await GetBlockChainAsync(electionId, isDesc: true);
            var options = await Node.Mempool.GetOptionsAsync(electionId);

            if (blockchain is null || blockchain.Count <= 1) return null;
            if (options is null || options.Count <= 1) return null;

            foreach (var b in blockchain) 
            {
                var s = Block.TryDecryptVote(b.EncryptedData, privateKey);
                if (s is not null && s.Equals(user.Username))
                {
                    var index = int.Parse(b.PublicData);
                    Option option = new Option()
                    {
                        OptionText = options[index - 1].OptionText,
                        Index = options[index - 1].Index,
                        ElectionId = options[index - 1].ElectionId
                    };

                    return option;
                }
            }

            return null;
        }

        public async Task<bool> ValidateBlockAsync(Block? block)
        {
            if (block is null) return false;

#if DEBUG
            Node.Output("after: is not null");
#endif

            var resp = await GetLatestBlockAsync(block.ElectionId);

            if (resp is null) return false;

#if DEBUG
            Node.Output("previ: is not null" );
#endif

            if (resp.ThisHash != block.PreviousHash) return false;

#if DEBUG
            Node.Output("Validation: Prev this hash fine");
#endif

            if (resp.Index + 1 != block.Index) return false;

#if DEBUG
            Node.Output("Validation: Index fine");
#endif

            if (block.ThisHash != block.CalculateHash()) return false;

#if DEBUG
            Node.Output("Validation: Calc hash fine");
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
                Node.Output("after: " + afterBlock.PreviousHash);
                Node.Output("previ: " + previousBlock.ThisHash);
#endif
                if (previousBlock.Index + 1 != afterBlock.Index)
                    return false;
#if DEBUG
                Node.Output("Validation: Index fine");
#endif
                if (afterBlock.ThisHash != afterBlock.CalculateHash())
                    return false;
#if DEBUG
                Node.Output("Validation: Calc hash fine");
#endif
                if (afterBlock.PreviousHash != previousBlock.ThisHash)
                    return false;
#if DEBUG
                Node.Output("Validation: Prev this hash fine");
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
                Node.Output("after: " + afterBlock.PreviousHash);
                Node.Output("previ: " + previousBlock.ThisHash);
#endif
                if (previousBlock.Index + 1 != afterBlock.Index)
                    return false;
#if DEBUG
                Node.Output("Validation: Index fine");
#endif
                if (afterBlock.ThisHash != afterBlock.CalculateHash())
                    return false;
#if DEBUG
                Node.Output("Validation: Calc hash fine");
#endif
                if (afterBlock.PreviousHash != previousBlock.ThisHash)
                    return false;
#if DEBUG
                Node.Output("Validation: Prev this hash fine");
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

            string query = "INSERT INTO block_chain_votes (election_id, index, timestamp, previous_hash, this_hash, nonce, difficulty, encrypted_data, public_data) " +
                           "VALUES (@election_id, @index, @timestamp, @previous_hash, @this_hash, @nonce, @difficulty, @encrypted_data, @public_data);";
            var parameters = new Dictionary<string, object>()
            {
                { "election_id", block.ElectionId },
                { "index", block.Index },
                { "timestamp", block.Timestamp },
                { "previous_hash", block.PreviousHash },
                { "this_hash", block.ThisHash },
                { "nonce", block.Nonce },
                { "difficulty", block.Difficulty },
                { "encrypted_data", block.EncryptedData },
                { "public_data", block.PublicData }
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
                Node.Output("Election is empty.");
                return;
            }

            if (options is null || options.Count == 0)
            {
                Node.Output("Can not upload options.");
                return;
            }

            if (elections is null || elections.Count == 0)
            {
                Node.Output("Can not upload elections.");
                return;
            }

            // Key - Username | Value - Option
            var uniqueResults = new Dictionary<string, string>();

            foreach (var result in blockchain) 
            {
                if (result.Index == 0) continue; // Skip genesis block

                if (!uniqueResults.TryAdd(result.EncryptedData, result.PublicData))
                {
                    uniqueResults[result.EncryptedData] = result.PublicData;
                }
            }

            var resultsCounter = new Dictionary<string, int>();

            foreach (var result in uniqueResults) 
            {
                if (resultsCounter.ContainsKey(result.Value))
                {
                    resultsCounter[result.Value]++;
                }
                else 
                { 
                    resultsCounter.Add(result.Value, 1);
                }
            }

            try
            {
                using FileStream fs = new FileStream($"results_id{electionId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.txt", FileMode.Create);
                string? electionName = elections.Find(e => e.Id == electionId)?.Name;
                electionName ??= "Unknown election";

                var title = $"Results for {electionName}\n";
                Node.Output(title);
                fs.Write(Encoding.UTF8.GetBytes(title));
                for (var i = 0; i < options.Count; i++)
                {
                    var line = $"{options[i].OptionText} - {(resultsCounter.ContainsKey((i + 1).ToString()) ? resultsCounter[(i + 1).ToString()] : "0")}\n";
                    Node.Output(line);
                    fs.Write(Encoding.UTF8.GetBytes(line));
                }
            }
            catch (Exception ex) 
            {
                Node.Output(ex.Message);
            }
        }
    }
}
