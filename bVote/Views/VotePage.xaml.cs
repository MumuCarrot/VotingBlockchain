using bVote.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VotingBlockchain.Datatypes;

namespace bVote.Views
{
    public partial class VotePage : Page
    {
        public VotePage(object context)
        {
            InitializeComponent();
            DataContext = context;
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ElectionViewModel)this.DataContext;
            if (viewModel.LoadOptionsCommand.CanExecute(null))
            {
                viewModel.LoadOptionsCommand.Execute(null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in ListBox.Items)
            {
                var container = ListBox.ItemContainerGenerator.ContainerFromItem(i) as ListBoxItem;
                if (container != null)
                {
                    var radio = FindVisualChild<RadioButton>(container);
                    radio.IsChecked = false;
                }
            }
        }

        public static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T tChild)
                    return tChild;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = (ElectionViewModel)this.DataContext;
            int option = -1;
            for (var i = 0; i < ListBox.Items.Count; i++)
            {
                var container = ListBox.ItemContainerGenerator.ContainerFromItem(ListBox.Items[i]) as ListBoxItem;
                if (container != null)
                {
                    var radio = FindVisualChild<RadioButton>(container);
                    if (radio?.IsChecked == true)
                    {
                        List<Option?> options = [];
                        foreach (var j in ListBox.ItemsSource)
                            options.Add(j as Option);
                        await viewModel.PostVote(options[i]!.Index);
                    }
                }
            }
        }
    }
}
