using VotingBlockchain.Interfaces;

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
                    Index = (int)i["id"],
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
                    ElectionIndex = (int)i["electionid"],
                    OptionText = (string)i["optiontext"]
                });
            }
            return votes;
        }

        private readonly List<IData> _inputMempool = [];
        private readonly List<IData> _outputMempool = [];

        public void AddToInputMempool(IData data) 
        {
            if (_inputMempool.Contains(data)) return;
            _inputMempool.Add(data);
        }

        public void AddToOutputMempool(IData data)
        {
            if (_outputMempool.Contains(data)) return;
            _outputMempool.Add(data);
        }

        public IData? GetInputData()
        {
            if (_inputMempool.Count == 0) return null;
            IData data = _inputMempool[0].Clone() as IData ?? throw new Exception("Can not clone block!");
            return data;
        }

        public IData? GetOutputData()
        {
            if (_outputMempool.Count == 0) return null;
            IData data = _outputMempool[0].Clone() as IData ?? throw new Exception("Can not clone block!");
            return data;
        }

        public void RemoveFromInputMempool(IData data)
        {
            if (_inputMempool.Count == 0) return;
            _inputMempool.Remove(data);
        }

        public void RemoveFromOutputMempool(IData data)
        {
            if (_outputMempool.Count == 0) return;
            _outputMempool.Remove(data);
        }
    }
}
