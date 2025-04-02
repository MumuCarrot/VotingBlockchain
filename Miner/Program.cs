namespace Miner
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            VotingBlockchain.Services.Miner miner = new VotingBlockchain.Services.Miner();

            miner.Start();

            Console.ReadLine();
        }
    }
}
