using Microsoft.UI.Xaml.Controls;
using PSB.Models;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.ContentDialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameSettingsContentDialog : ContentDialog
    {
        GameSettingsContentViewModel GameSettingsContentViewModel { get; set; }
        public GameSettingsContentDialog(Game game)
        {
            this.InitializeComponent();
            GameSettingsContentViewModel = new GameSettingsContentViewModel(game);
        }
    }
}
