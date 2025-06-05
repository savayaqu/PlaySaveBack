using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels.Auth;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Auth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RestoreFromMailPage : Page
    {
        public RestoreFromMailViewModel RestoreFromMailViewModel { get; private set; }
        public RestoreFromMailPage()
        {
            RestoreFromMailViewModel = new RestoreFromMailViewModel();
            this.InitializeComponent();
        }
    }
}
