using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels.Auth;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Auth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RestoreFromKeyPage : Page
    {
        public RestoreFromKeyViewModel RestoreFromKeyViewModel { get; private set; }
        public RestoreFromKeyPage()
        {
            RestoreFromKeyViewModel = new RestoreFromKeyViewModel();
            this.InitializeComponent();
        }
    }
}
