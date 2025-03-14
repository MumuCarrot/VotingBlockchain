namespace VotingBlockchain
{
    public class Mempool
    {
        private readonly List<Block> _inputMempool = [];

        public void AddToInputMempool(Block data)
        {
            if (_inputMempool.Contains(data)) return;
            _inputMempool.Add(data);
        }

        public Block? GetInputData()
        {
            if (_inputMempool.Count == 0) return null;
            Block data = _inputMempool[0];
            return data;
        }

        public bool RemoveFromInputMempool(Block data)
        {
            if (_inputMempool.Count == 0) return false;

            var blockToRemove = _inputMempool.FirstOrDefault(b => b.ElectionId == data.ElectionId && b.Index == data.Index);

            if (blockToRemove is null) return false;
            _inputMempool.Remove(blockToRemove);

            return true;
        }
    }
}
