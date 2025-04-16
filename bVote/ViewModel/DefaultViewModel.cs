using bVote.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using bVote.Pages;
using System.Windows;
using VotingBlockchain;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bVote.ViewModel
{
    public partial class DefaultViewModel : ObservableObject
    {
        private readonly NavigationService _navigation;

        [ObservableProperty]
        private string _login = "";

        public ICommand DefaultCommand { get; }

        public DefaultViewModel(NavigationService navigation)
        {
            _navigation = navigation;
            DefaultCommand = new RelayCommand(async () => await ValidateAndNavigateAsync());
        }

        [GeneratedRegex(@"^(?:[А-ЩЬЮЯҐЄІЇ]{2}\d{6}|\d{9})$")]
        private static partial Regex UkrainianStandartPasspordRegex();

        private async Task ValidateAndNavigateAsync()
        {
            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Login cannot be empty!");
                return;
            }

            if (!UkrainianStandartPasspordRegex().IsMatch(Login))
            {
                MessageBox.Show("Login does not match the required format!");
                return;
            }

            if (await Client.UserExists(Login))
            {
                _navigation.NavigateTo(new LoginPage(_navigation, Login));
            }
            else
            {
                _navigation.NavigateTo(new RegisterPage(_navigation, Login));
            }
        }
    }
}
