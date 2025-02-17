using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Mempool
    {
        private readonly List<IData> _waitForHashMempool = [];
        private readonly List<IData> _dataMempool = [];
        public List<IData> DataMempool { get { return _dataMempool; } }

        public void AddToHashWaitingMempool(IData block) 
        {
            _waitForHashMempool.Add(block);
        }

        public IData? GetHashWaitingData() 
        {
            if (_waitForHashMempool.Count > 0) 
            { 
                IData block = _waitForHashMempool[0].Clone() as IData ?? throw new Exception("Can not clone block!");
                _waitForHashMempool.RemoveAt(0);
                return block;
            }
            return null;
        }

        public void AddToDataMempool(IData data)
        {
            _dataMempool.Add(data);
        }
    }
}
