namespace VotingBlockchain.Abstract
{
    public abstract class AService
    {
        protected Task? serviceTask;
        protected bool isRunning;

        public abstract void Start();
        public abstract void Stop();
    }
}
