using System.Windows.Controls;

namespace bVote.Views
{
    public partial class MyVotePage : Page
    {
        public MyVotePage(object context)
        {
            InitializeComponent();
            DataContext = context;
        }
    }
}
