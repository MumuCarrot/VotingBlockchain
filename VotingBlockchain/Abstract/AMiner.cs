using VotingBlockchain.Interfaces;

namespace VotingBlockchain.Abstract
{
    public abstract class AMiner : IMiner
    {
        protected Thread? minerThread;
        protected readonly object lockObject = new();
        protected bool isMining;

        public abstract void Mine();

        public abstract void Rest();
    }
}
