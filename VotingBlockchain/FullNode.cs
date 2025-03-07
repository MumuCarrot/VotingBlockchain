using VotingBlockchain.Abstract;

namespace VotingBlockchain
{
    public class FullNode : ANode
    {
        public override void HostServer() 
        {
            this.Output("Server hosted");
        }
    }
}
