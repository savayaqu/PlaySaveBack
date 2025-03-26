using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeneralPage : Page
    {
        public GeneralViewModel GeneralViewModel { get; set; } = App.MainWindow!.GeneralViewModel;
        public GeneralPage()
        {
            this.InitializeComponent();
            DataContext = GeneralViewModel;
        }
    }
}
