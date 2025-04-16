using bVote.Services;
using bVote.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using VotingBlockchain;
using VotingBlockchain.Datatypes;

namespace bVote.ViewModel
{
    public partial class ElectionViewModel : ObservableObject
    {
        private readonly NavigationService _navigation;
        private Election _election;

        public int Id => _election.Id;

        public string Name { get => _election.Name; set { _election.Name = value; OnPropertyChanged(); } }

        public long StartDate => _election.StartDate;

        public long EndDate => _election.EndDate;

        public string Description { get => _election.Description; set { _election.Description = value; OnPropertyChanged(); } }

        [ObservableProperty]
        public string _privateKey = "";

        private string _result = "";
        public string Result 
        { 
            get => _result; 
            set 
            {
                _result = value;
                OnPropertyChanged();
            } 
        }

        private List<Option> _options = [];
        public List<Option> Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenElectionPageCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand GoToMyVoteCommand { get; }
        public ICommand ClearResultCommand { get; }
        public ICommand GetMyVoteCommand { get; }
        public ICommand LoadOptionsCommand { get; }
        public ICommand VoteCommand { get; }
        public ICommand ResultsCommand { get; }

        public ElectionViewModel(NavigationService navigationService, Election election)
        {
            _navigation = navigationService;
            _election = election;

            BackToMenuCommand = new RelayCommand(() => _navigation.NavigateTo(new MenuPage(_navigation)));
            OpenElectionPageCommand = new RelayCommand(() => _navigation.NavigateTo(new ElectionPage(new ElectionViewModel(_navigation, _election))));
            GoToMyVoteCommand = new RelayCommand(() => _navigation.NavigateTo(new MyVotePage(this)));
            ClearResultCommand = new RelayCommand(() => Result = "");
            GetMyVoteCommand = new RelayCommand(async () => await GetMyVote());
            LoadOptionsCommand = new RelayCommand(async () => await GetOptions());
            VoteCommand = new RelayCommand(() => _navigation.NavigateTo(new VotePage(this)));
            ResultsCommand = new RelayCommand(async () => await GetResults());
        }

        private async Task GetMyVote() 
        {
            if (PrivateKey.Length < 100) return;
            var option = await Client.GetUserVote(Client.User.Username, Id, PrivateKey);
            if (option is null) return;
            Result = option.OptionText;
        }

        private async Task GetOptions()
        {
            var options = await Client.GetOptions(Id);
            if (options is null) return;
            Options = options;
        }

        private async Task GetResults() 
        {
            var resultDict = await Client.GetBlocks(Id);
            ResultsWindow rw = new ResultsWindow(resultDict);
            rw.Show();
        }

        public async Task PostVote(int option) 
        {
            await Client.Vote(Id, int.Parse(Client.User.Id), Client.User.Username, Client.User.PublicKey, option);
        }
    }
}
