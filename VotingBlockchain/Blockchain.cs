//#define DEBUG

using System.Text;
using System.Text.Json;
using VotingBlockchain.Datatypes;

namespace VotingBlockchain
{
    public partial class FullNode
    {
        public partial class Blockchain
        {
            private FullNode Node { get; }

            public Blockchain(FullNode node)
            {
                Node = node;
            }

            public async Task<Option?> GetUserVoteAsync(string username, int electionId, string privateKey)
            {
                var blockchain = await DBQuery.GetBlockChainAsync(electionId, isDesc: true);
                var options = await DBQuery.GetOptionsAsync(electionId);

                if (blockchain is null || blockchain.Count <= 1) return null;
                if (options is null || options.Count <= 1) return null;

                foreach (var b in blockchain)
                {
                    var s = Block.TryDecryptVote(b.EncryptedData, privateKey);
                    if (s is not null && s.Equals(username))
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
                Node.Output("previ: is not null");
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

            public async Task VoteAsync(int electionId, string username, string publicKey, int option)
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

                Node.Mempool.AddToInputMempool(new Block(resp.Index + 1, resp.ElectionId, resp.ThisHash, username, option.ToString(), publicKey));
            }

            public async Task AddBlockAsync(Block block)
            {
                if (!await ValidateBlockAsync(block)) return;

                if (!Node.Mempool.RemoveFromInputMempool(block)) return;

                await DBQuery.AddBlockAsync(block);
            }
        }
    }
}
