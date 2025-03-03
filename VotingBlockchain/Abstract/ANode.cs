using VotingBlockchain.Interfaces;

namespace VotingBlockchain.Abstract
{
    public abstract class ANode : INode
    {
        public Mempool Mempool { get; }
        public Blockchain Blockchain { get; }
        public UserDatabase UserDatabase { get; }

        public ANode() 
        { 
            Blockchain = new Blockchain(this);
            Mempool = new Mempool();
            UserDatabase = new UserDatabase();
        }

        public abstract void HostServer();
    }
}
