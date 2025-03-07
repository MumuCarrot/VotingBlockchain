using VotingBlockchain.Interfaces;

namespace VotingBlockchain.Abstract
{
    public abstract class ANode : INode
    {
        public Output _output;
        public Mempool Mempool { get; }
        public Blockchain Blockchain { get; }
        public UserDatabase UserDatabase { get; }

        public ANode() 
        {
            var dba = new DBAdapter();
            _output = new Output(Console.WriteLine);
            Blockchain = new Blockchain(dba, this);
            Mempool = new Mempool(dba);
            UserDatabase = new UserDatabase(dba);
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
