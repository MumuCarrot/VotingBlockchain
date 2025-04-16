using bVote.Enums;
using bVote.Services;
using bVote.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VotingBlockchain;
using VotingBlockchain.Datatypes;

namespace bVote.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigation;
        public event PropertyChangedEventHandler? PropertyChanged;

        private ElectionType Type { get; }

        private ObservableCollection<ElectionViewModel> _elections = new();
        public ObservableCollection<ElectionViewModel> Elections
        {
            get => _elections;
            private set
            {
                _elections = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchElectionsCommand { get; }
        public ICommand BackToMenuCommand { get; }

        public HomeViewModel(NavigationService navigation, ElectionType electionType)
        {
            _navigation = navigation;

            Type = electionType;

            SearchElectionsCommand = new RelayCommand(async () => await SearchElections());
            BackToMenuCommand = new RelayCommand(() => _navigation.NavigateTo(new MenuPage(_navigation)));
        }

        public async Task SearchElections()
        {
            List<Election>? el = [];
            switch (Type)
            {
                case ElectionType.Voted:
                    el = await Client.GetVotedElections(int.Parse(Client.User?.Id ?? "0"));
                    break;
                case ElectionType.Completed:
                    el = await Client.GetCompletedElections(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    break;
                default:
                    el = await Client.GetCurrentElections(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    break;
            }
            if (el is not null)
                foreach (var i in el)
                    Elections.Add(new ElectionViewModel(_navigation, i));
            else
                MessageBox.Show("No elections found.");
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
