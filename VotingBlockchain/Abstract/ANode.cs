using VotingBlockchain.Interfaces;

namespace VotingBlockchain.Abstract
{
    public abstract class ANode : INode
    {
        private Output _output;
        private List<AService> _services = [];

        public Mempool Mempool { get; }
        public Blockchain Blockchain { get; }
        

        public ANode() 
        {
            Blockchain = new Blockchain(this);
            Mempool = new Mempool();

            _output = new Output(Console.WriteLine);
            _services.Add(new ElectionService());
        }

        public void SetOutput(Action<string> action) 
        {
            _output = new Output(action);
        }

        public void Output(string message)
        {
            _output.Write(message);
        }

        public abstract void HostServer();
    }
}
