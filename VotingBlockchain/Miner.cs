using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using VotingBlockchain.Abstract;
using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Miner: AMiner
    {
        public INode Node { get; }

        public Miner(INode node) 
        { 
            Node = node;
        }

        public override void Mine() 
        {
            if (minerThread == null || !minerThread.IsAlive)
            {
                isMining = true;
                minerThread = new Thread(new ThreadStart(() =>
                {
                    while (isMining)
                    {
                        try
                        {
                            var data = Node.Mempool.GetHashWaitingData();
                            if (data is Vote vote)
                            {
                                var lastBlock = Node.Blockchain.GetLatestBlock().Clone() as Block;

                                if (lastBlock is null) return;

                                lastBlock.Vote = vote;
                                lastBlock = SearchHash(lastBlock) as Block;

                                if (lastBlock is null) return;

                                Node.Blockchain.AddBlock(lastBlock);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Mining error: {ex.Message}");
                        }

                        Thread.Sleep(5000);
                    }
                }))
                {
                    IsBackground = true
                };
                minerThread.Start();
            }
        }

        public override void Rest() 
        {
            isMining = false;
            minerThread?.Join();
        }

        public IBlock SearchHash(IBlock block)
        {
            string hash = new string('1', block.Difficulty);
            while (hash[..block.Difficulty] != new string('0', block.Difficulty))
            {
                string blockData = block.Index + block.Timestamp.ToString() + block.Nonce + block.Difficulty + block.PreviousHash + JsonSerializer.Serialize(block.Vote);
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                    hash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
                block.Nonce++;
            }
            block.Nonce--;
            block.ThisHash = hash;
            Console.WriteLine(hash);
            return block;
        }
    }
}
