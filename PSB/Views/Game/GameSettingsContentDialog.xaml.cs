using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.Interfaces;
using PSB.ViewModels;

namespace PSB.ContentDialogs
{

    public sealed partial class GameSettingsContentDialog : ContentDialog
    {
        GameSettingsContentViewModel GameSettingsContentViewModel { get; set; }
        public GameSettingsContentDialog(IGame game, GameViewModel gameViewModel)
        {
            GameSettingsContentViewModel = new GameSettingsContentViewModel(game, gameViewModel);
            this.InitializeComponent();
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
