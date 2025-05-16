using VotingBlockchain.Datatypes.Classes;
using VotingBlockchain.Enums;

namespace VotingBlockchain.Mempool
{
    public class Mempool
    {
        private readonly List<MempoolItem> _inputMempool = [];

        public void AddToInputMempool(Block data)
        {
            if (_inputMempool.Find(i => i.preInfo == data) is not null) return;
            _inputMempool.Add(new MempoolItem(data));
        }

        public MempoolItem? GetInputData()
        {
            if (_inputMempool.Count == 0) return null;
            return _inputMempool[0];
        }

        public bool RemoveFromInputMempool(Block data)
        {
            if (_inputMempool.Count == 0) return false;

            var blockToRemove = _inputMempool.FirstOrDefault(i => i.preInfo.ElectionId == data.ElectionId && i.preInfo.Index == data.Index);

            if (blockToRemove is null) return false;
            _inputMempool.Remove(blockToRemove);

            return true;
        }
    }

    public record MempoolItem (Block preInfo);
}
