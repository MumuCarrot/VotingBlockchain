using bVote.Services;
using bVote.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;
using VotingBlockchain.Client;
using VotingBlockchain.Datatypes.Classes;

namespace bVote.ViewModel
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly NavigationService _navigation;

        [ObservableProperty]
        private string _login = "";

        public string Password = "";
        public string CPassword = "";

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(NavigationService navigation, string preLogin = "")
        {
            _navigation = navigation;
            _login = preLogin;
            RegisterCommand = new RelayCommand(async () => await ValidateAndNavigateAsync());
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

            if (string.IsNullOrWhiteSpace(CPassword))
            {
                MessageBox.Show("Confirm Password cannot be empty!");
                return;
            }

            if (Password.Length <= 6)
            {
                MessageBox.Show("Password too short!");
                return;
            }

            if (Password != CPassword)
            {
                MessageBox.Show("Password and CPassword should be same!");
                return;
            }

            var keys = await Client.Register(Login, Password) ?? throw new Exception("Keys is null");

            using (FileStream fs = new FileStream($"{Login}_pkey.txt", FileMode.Create)) 
            {
                fs.Write(Encoding.UTF8.GetBytes(keys[1]));
            }

            User? user = await Client.Login(Login, Password);

            if (user is null) return;

            Client.User = user;

            _navigation.NavigateTo(new MenuPage(_navigation));
        }
    }
}
