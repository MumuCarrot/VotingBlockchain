#define DEBUG

using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Blockchain
    {
        protected List<Block> Chain { get; set; }
        private INode Node { get; }

        public Blockchain(INode node)
        {
            Node = node;
            Chain = [Block.CreateGenesisBlock()];
            Chain[0].ThisHash = Chain[0].CalculateHash();
            //CloseBlockWith(Chain[0]);
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

        public void VoteOrUpdate(User user, string candidate)
        {

        }
    }
}
