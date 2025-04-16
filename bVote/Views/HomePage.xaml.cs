using bVote.Services;
using bVote.ViewModel;
using System.Windows.Controls;

namespace bVote.Views
{
    public partial class HomePage : Page
    {
        public HomePage(object model)
        {
            InitializeComponent();
            DataContext = model;
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (HomeViewModel)this.DataContext;
            if (viewModel.SearchElectionsCommand.CanExecute(null))
            {
                viewModel.SearchElectionsCommand.Execute(null);
            }
        }
    }
}
