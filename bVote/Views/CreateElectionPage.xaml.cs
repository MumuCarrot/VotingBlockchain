using bVote.Services;
using System.Windows.Controls;
using bVote.ViewModel;

namespace bVote.Views
{
    public partial class CreateElectionPage : Page
    {
        public CreateElectionPage(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new CreateElectionViewModel(navigationService);
        }
    }
}
