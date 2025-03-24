using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountPage : Page
    {
        public AccountViewModel AccountViewModel => App.MainWindow!.AccountViewModel;
        public ProfileViewModel ProfileViewModel => App.MainWindow!.ProfileViewModel;
        public AccountPage()
        {
            this.InitializeComponent();
            DataContext = AccountViewModel;
        }
    }
}
