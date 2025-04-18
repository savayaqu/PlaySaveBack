using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PSB.Models;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ProfileViewModel { get; set; }
        public ProfilePage()
        {
            this.InitializeComponent();
            ProfileViewModel = App.MainWindow!.ProfileViewModel;
        }
        private void OnGameTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Library library)
            {
                if (library.Game != null)
                {
                    string gameTag = $"Game_{library.Game.Id}|{library.Game.Name}";
                    App.NavigationService!.Navigate(gameTag);
                }
                if (library.SideGame != null)
                {
                    string gameTag = $"SideGame_{library.SideGame.Id}|{library.SideGame.Name}";
                    App.NavigationService!.Navigate(gameTag);
                }
            }
        }
    }
}
