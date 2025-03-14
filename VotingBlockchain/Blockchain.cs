//#define DEBUG

using System.Text;
using VotingBlockchain.Abstract;
using VotingBlockchain.Datatypes;

namespace VotingBlockchain
{
    public class Blockchain
    {
        private ANode Node { get; }

        public Blockchain(ANode node)
        {
            Node = node;
        }

        public async Task<Option?> GetUserVoteAsync(User user, int electionId, string privateKey)
        {
            var blockchain = await DBQuery.GetBlockChainAsync(electionId, isDesc: true);
            var options = await DBQuery.GetOptionsAsync(electionId);

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

            var resp = await DBQuery.GetLatestBlockAsync(block.ElectionId);

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
            var blockChain = await DBQuery.GetBlockChainAsync(electionId);

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
            var blockChain = await DBQuery.GetBlockChainAsync(newBlock.ElectionId);

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
            var election = await DBQuery.GetElectionAsync(electionId);

            if (election is null) return;

            if (election.EndDate < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                Node.Output("This election is closed.");
                return;
            }

            var resp = await DBQuery.GetLatestBlockAsync(electionId);

            if (resp is null) return;

            Node.Mempool.AddToInputMempool(new Block(resp.Index + 1, resp.ElectionId, resp.ThisHash, user.Username, option.ToString(), user.PublicKey));
        }

        public async Task AddBlockAsync(Block block)
        {
            if (!await ValidateBlockAsync(block)) return;

            if (!Node.Mempool.RemoveFromInputMempool(block)) return;

            await DBQuery.AddBlockAsync(block);
        }

        public async Task GetElectionResults(int electionId) 
        {
            var blockchain = await DBQuery.GetBlockChainAsync(electionId);
            var options = await DBQuery.GetOptionsAsync(electionId);
            var elections = await DBQuery.GetElectionsAsync();

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
