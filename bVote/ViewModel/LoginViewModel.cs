using bVote.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Windows;
using VotingBlockchain;
using bVote.Services;
using VotingBlockchain.Datatypes;
using System.ComponentModel;

namespace bVote.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly NavigationService _navigation;

        [ObservableProperty]
        private string _login = "";

        public string Password = "";

        public ICommand LoginCommand { get; }

        public LoginViewModel(NavigationService navigation, string preLogin = "")
        {
            _navigation = navigation;
            _login = preLogin;
            LoginCommand = new RelayCommand(async () => await ValidateAndNavigateAsync());
        }

        private async Task ValidateAndNavigateAsync()
        {
            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Login cannot be empty!");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Password cannot be empty!");
                return;
            }

            if (Password.Length <= 6) 
            {
                MessageBox.Show("Password too short!");
                return;
            }

            User? user = await Client.Login(Login, Password);

            if (user is null) return;

            Client.User = user;

            _navigation.NavigateTo(new MenuPage(_navigation));
        }
    }
}
