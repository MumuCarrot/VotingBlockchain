#define DEBUG

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
            if (minerThread == null || minerThread.IsCompleted)
            {
                isMining = true;
                minerThread = new Task(async () =>
                {
                    while (isMining)
                    {
                        try
                        {
                            var data = Node.Mempool.GetInputData();

                            if (data is not null) 
                            { 
                                data = SearchHash(data);

                                await Node.Blockchain.AddBlockAsync(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Mining error: {ex.Message}");
                        }

                        await Task.Delay(5000);
                    }
                });
                minerThread.Start();
            }
        }

        public override void Rest() 
        {
            isMining = false;
            minerThread?.Wait();
        }

        public Block SearchHash(Block block)
        {
            string hash = new string('1', block.Difficulty);
            while (hash[..block.Difficulty] != new string('0', block.Difficulty))
            {
                string blockData = block.Index + block.ElectionId + block.Timestamp.ToString() + block.Nonce + block.Difficulty + block.PreviousHash + block.EncryptedData;
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
