using System.Windows.Controls;
using bVote.ViewModel;
using bVote.Services;
using System.Windows;

namespace bVote.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage(NavigationService navigationService, string preLogin = "")
        {
            InitializeComponent();
            DataContext = new LoginViewModel(navigationService, preLogin);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
