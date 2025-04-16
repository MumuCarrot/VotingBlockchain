using System.Windows.Controls;

namespace bVote.Views
{
    public partial class ElectionPage : Page
    {
        public ElectionPage(object context)
        {
            InitializeComponent();
            DataContext = context;
        }
    }
}
