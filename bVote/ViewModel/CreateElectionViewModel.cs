using bVote.Services;
using bVote.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using VotingBlockchain.Client;
using VotingBlockchain.Datatypes.Classes;

namespace bVote.ViewModel
{
    public partial class CreateElectionViewModel : ObservableObject
    {
        private readonly NavigationService _navigation;
        private Election _election;

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private string _startDate = "";

        [ObservableProperty]
        private string _endDate = "";

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private string _nCounter = "";

        [ObservableProperty]
        private string _options = "";

        public ICommand BackToMenuCommand { get; }
        public ICommand CreateElectionCommand { get; }
        public ICommand ClearResultCommand { get; }

        public CreateElectionViewModel(NavigationService navigationService)
        {
            _navigation = navigationService;

            BackToMenuCommand = new RelayCommand(() => _navigation.NavigateTo(new MenuPage(_navigation)));
            CreateElectionCommand = new RelayCommand(async () => await PostElection());
            ClearResultCommand = new RelayCommand(() => Clear());
        }

        private void Clear() 
        {
            Name = "";
            StartDate = "";
            EndDate = "";
            Description = "";
            NCounter = "";
            Options = "";
        }

        private async Task PostElection()
        {
            await Client.PostElection(Name, long.Parse(StartDate), long.Parse(EndDate), Description, int.Parse(NCounter), Options);
        }
    }
}
