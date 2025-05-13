using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

namespace PSB.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel HomeViewModel { get; set; }
        public HomePage()
        {
            HomeViewModel = new HomeViewModel();
            this.InitializeComponent();
        }
    }
}
