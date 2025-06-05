using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

namespace PSB.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel HomeViewModel { get; set; }
        public GameViewModel GameViewModel { get; set; }
        public HomePage()
        {
            HomeViewModel = new HomeViewModel();
            if (HomeViewModel.LastPlayedGame != null)
            {
                GameViewModel = new GameViewModel(HomeViewModel.LastPlayedGame.Game.Id, HomeViewModel.LastPlayedGame.Game.Type);
            }
            this.InitializeComponent();
        }
    }
}
