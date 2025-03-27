using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

namespace PSB.Views.Settings
{
    public sealed partial class AccountPage : Page
    {
        public AccountViewModel AccountViewModel { get; set; } = App.MainWindow!.AccountViewModel; 
        public ProfileViewModel ProfileViewModel => App.MainWindow!.ProfileViewModel;
        public AccountPage()
        {
            this.InitializeComponent();
            DataContext = AccountViewModel;
        }
    }
}
