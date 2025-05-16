using bVote.Services;
using bVote.ViewModel;
using System.Windows.Controls;
using VotingBlockchain.Client;

namespace bVote.Views
{
    public partial class MenuPage : Page
    {
        NavigationService _navigationService;
        public MenuPage(NavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
            DataContext = new MenuViewModel(navigationService);
            CreateBtn.Visibility = Client.User.Role == 0 ? 
                System.Windows.Visibility.Collapsed :
                System.Windows.Visibility.Visible;
        }
    }
}
