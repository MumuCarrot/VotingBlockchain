using System.Windows;
using System.Windows.Controls;
using bVote.Services;
using bVote.ViewModel;

namespace bVote.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage(NavigationService navigationService, string preLogin = "")
        {
            InitializeComponent();
            DataContext = new RegisterViewModel(navigationService, preLogin);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void CPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.CPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
