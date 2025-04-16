using System.Windows;
using System.Windows.Controls;
using bVote.ViewModel;
using bVote.Services;

namespace bVote.Pages
{
    public partial class DefaultPage : Page
    {
        public DefaultPage(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new DefaultViewModel(navigationService);
        }
    }
}
