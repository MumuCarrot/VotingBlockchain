using bVote.Pages;
using bVote.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace bVote.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public NavigationService Navigation { get; }

        public MainViewModel()
        {
            Navigation = new NavigationService();

            Navigation.NavigateTo(new DefaultPage(Navigation));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
