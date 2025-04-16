using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bVote.Services;
using bVote.Views;
using bVote.Enums;
using CommunityToolkit.Mvvm.Input;

namespace bVote.ViewModel
{
    public class MenuViewModel
    {
        NavigationService _navigation;

        public ICommand CurrentVotesCommand { get; }
        public ICommand ResultVotesCommand { get; }
        public ICommand SeeMyVotesCommand { get; }
        public ICommand CreateElectionCommand { get; }

        public MenuViewModel(NavigationService navigationService) 
        {
            _navigation = navigationService;

            CurrentVotesCommand = new RelayCommand(() => _navigation.NavigateTo(new HomePage(new HomeViewModel(navigationService, ElectionType.Current))));
            ResultVotesCommand = new RelayCommand(() => _navigation.NavigateTo(new HomePage(new HomeViewModel(navigationService, ElectionType.Completed))));
            SeeMyVotesCommand = new RelayCommand(() => _navigation.NavigateTo(new HomePage(new HomeViewModel(navigationService, ElectionType.Voted))));
            CreateElectionCommand = new RelayCommand(() => _navigation.NavigateTo(new CreateElectionPage(navigationService)));
        }
    }
}
