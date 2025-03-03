using System.Text.Json;
using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Blockchain
    {
        protected List<Block> Chain { get; set; }
        private INode Node { get; }
        private const string FilePath = "blockchain.bin";

        public Blockchain(INode node)
        {
            Node = node;
            var lff = LoadFromFile();
            if (lff is not null)
            {
                Chain = lff;
            }
            else
            {
                Chain = [Block.CreateGenesisBlock()];
                Chain[0].ThisHash = Chain[0].CalculateHash();
                CloseBlockWith(Chain[0]);
            }
        }

        public Block GetLatestBlock() => Chain[^1];

        public bool Validation(Block newBlock)
        {
            var newChain = new List<Block>(Chain);
            newChain[^1] = newBlock;
            for (int i = 1; i < newChain.Count; i++)
            {
                Block previousBlock = newChain[i - 1];
                Block afterBlock = newChain[i];

                Console.WriteLine("after: " + afterBlock.PreviousHash);
                Console.WriteLine("previ: " + previousBlock.ThisHash);

                if (previousBlock.Index + 1 != afterBlock.Index)
                    return false;

                Console.WriteLine("Validation: Index fine");

                if (afterBlock.ThisHash != afterBlock.CalculateHash())
                    return false;

                Console.WriteLine("Validation: Calc hash fine");

                if (afterBlock.PreviousHash != previousBlock.ThisHash)
                    return false;

                Console.WriteLine("Validation: Prev this hash fine");
            }
            return true;
        }

        public void VoteOrUpdate(User user, string candidate)
        {
            if (user is null) return;

            Vote newVote = new Vote(user.Id, candidate);

            var existingVote = Node.Mempool.DataMempool.FirstOrDefault(v => v.UserId == newVote.UserId);

            if (existingVote is not null)
                existingVote.Candidate = newVote.Candidate;
            else
                Node.Mempool.AddToDataMempool(newVote);
        }

        public void EndVoting(User user)
        {
            var existingVote = Node.Mempool.DataMempool.FirstOrDefault(v => v.UserId == user.Id) ??
                throw new Exception("The user's vote was not found");
            Node.Mempool.AddToHashWaitingMempool(existingVote);
        }

        public bool AddBlock(Block newBlock)
        {
            if (!Validation(newBlock))
                return false;
            Console.WriteLine("Block valid");

            var existingVote = Node.Mempool.DataMempool.FirstOrDefault(v => v.UserId == newBlock.Vote.UserId);
            if (existingVote is null)
                return false;
            Console.WriteLine("Vote exist");

            Node.Mempool.DataMempool.Remove(existingVote);
            CloseBlockWith(newBlock);
            SaveToFile();
            return true;
        }

        public void CloseBlockWith(Block block)
        {
            Chain[^1] = block;
            Chain.Add(new Block(Chain.Count, Chain[^1].ThisHash));
        }

        public string? GetUserVote(User user)
        {
            if (user is null) return null;

            foreach (var block in Chain)
            {
                if (block.Vote?.UserId == user.Id)
                {
                    return block.Vote.Candidate;
                }
            }
            return null;
        }

        public void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            var json = JsonSerializer.Serialize(Chain, options);
            FileEncryption.EncryptToFile(FilePath, json);
        }

        private List<Block>? LoadFromFile()
        {
            string decryptedJson = FileEncryption.DecryptFromFile(FilePath);
            return !string.IsNullOrEmpty(decryptedJson) ? JsonSerializer.Deserialize<List<Block>>(decryptedJson) : null;
        }

        public VotingResults GetCurrentResults()
        {
            return new VotingResults(Chain);
        }
    }
}
