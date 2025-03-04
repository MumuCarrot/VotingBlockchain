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
            var dba = new DBAdapter();
            Blockchain = new Blockchain(this);
            Mempool = new Mempool(dba);
            UserDatabase = new UserDatabase(dba);
        }

        public abstract void HostServer();
    }
}
