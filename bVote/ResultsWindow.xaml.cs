using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VotingBlockchain;
using VotingBlockchain.Datatypes;
using WpfDrawing.Charts;

namespace bVote
{
    public partial class ResultsWindow : Window
    {
        List<Block> _blockchain;
        public ResultsWindow(List<Block> blockchain)
        {
            InitializeComponent();
            _blockchain = blockchain;
        }

        private void CreateChart(Chart chart)
        {
            chart.Clear();

            Dictionary<string, int> res = new();

            foreach (var b in _blockchain)
            {
                if (b.PublicData == "0") continue;
                if (!res.TryAdd(b.PublicData, 1))
                {
                    res[b.PublicData]++;
                }
            }

            foreach (var d in res)
            {
                chart.AddValue(d.Value);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var electionId = _blockchain[0].ElectionId;
            var options = await Client.GetOptions(electionId);
            var elections = await Client.GetElections();

            if (options is null || options.Count == 0)
            {
                return;
            }

            if (elections is null || elections.Count == 0)
            {
                return;
            }

            // Key - Username | Value - Option
            var uniqueResults = new Dictionary<string, string>();

            foreach (var result in _blockchain)
            {
                if (result.Index == 0) continue; // Skip genesis block

                if (!uniqueResults.TryAdd(result.EncryptedData, result.PublicData))
                {
                    uniqueResults[result.EncryptedData] = result.PublicData;
                }
            }

            var resultsCounter = new Dictionary<string, int>();

            foreach (var result in uniqueResults)
            {
                if (resultsCounter.ContainsKey(result.Value))
                {
                    resultsCounter[result.Value]++;
                }
                else
                {
                    resultsCounter.Add(result.Value, 1);
                }
            }

            try
            {
                using FileStream fs = new FileStream($"results_id{electionId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.txt", FileMode.Create);
                string? electionName = elections.Find(e => e.Id == electionId)?.Name;
                electionName ??= "Unknown election";

                var title = $"Results for {electionName}\n";
                fs.Write(Encoding.UTF8.GetBytes(title));
                for (var i = 0; i < options.Count; i++)
                {
                    var line = $"{options[i].OptionText} - {(resultsCounter.ContainsKey((i + 1).ToString()) ? resultsCounter[(i + 1).ToString()] : "0")}\n";
                    fs.Write(Encoding.UTF8.GetBytes(line));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GridForChart.Children.OfType<Canvas>().ToList().ForEach(p => GridForChart.Children.Remove(p));
            Chart chart = new BarChart();
            GridForChart.Children.Add(chart.ChartBackground);
            GridForChart.UpdateLayout();
            CreateChart(chart);
        }
    }
}
