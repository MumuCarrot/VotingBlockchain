using System;

namespace VotingBlockchain
{
    public class Mempool
    {
        private readonly DBAdapter _adapter;
        public Mempool(DBAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task<List<Election>?> GetElectionsAsync()
        {
            string query = "SELECT * FROM elections";
            var dict = await _adapter.ExecuteQueryAsync(query);

            if (dict is null || dict.Count == 0) return null;

            var elections = new List<Election>();
            foreach (var i in dict)
            {
                elections.Add(new Election()
                {
                    Id = (int)i["id"],
                    Name = (string)i["name"],
                    StartDate = (long)i["startdate"],
                    EndDate = (long)i["enddate"]
                });
            }
            return elections;
        }

        public async Task<List<Option>?> GetOptionsAsync(int id)
        {
            string query = "SELECT * FROM options WHERE electionid = @id";
            var parameters = new Dictionary<string, object>()
            {
                { "id", id }
            };
            var dict = await _adapter.ExecuteQueryAsync(query, parameters);
            if (dict is null || dict.Count == 0) return null;
            var votes = new List<Option>();
            foreach (var i in dict)
            {
                votes.Add(new Option()
                {
                    Index = (int)i["id"],
                    ElectionId = (int)i["electionid"],
                    OptionText = (string)i["optiontext"]
                });
            }
            return votes;
        }

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
