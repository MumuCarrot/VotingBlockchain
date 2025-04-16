using bVote.ViewModel;
using System.Windows;

namespace bVote;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }


}