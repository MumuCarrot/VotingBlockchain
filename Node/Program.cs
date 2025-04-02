using VotingBlockchain;

namespace Node
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            FullNode fullNode = new FullNode();
            fullNode.SetOutput(Console.WriteLine);
            await fullNode.HostServer();
        }
    }
}
