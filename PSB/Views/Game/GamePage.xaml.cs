using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PSB.Models;
using PSB.ViewModels;


namespace PSB.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GameViewModel? GameViewModel { get; set; }
        public GamePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is GameNavigationParameters parameters)
            {
                if (GameViewModel == null || GameViewModel.GameId != parameters.GameId)
                {
                    GameViewModel = new GameViewModel(parameters.GameId, parameters.Type);
                    DataContext = GameViewModel;
                    GameViewModel.GameLoaded += OnGameLoaded;
                }
            }
        }

        private void OnGameLoaded()
        {
            if (GameViewModel?.Game != null)
            {
                App.MainWindow!.HeaderTextBlock.Text = GameViewModel.Game.Name;
                App.NavigationService!.SyncNavigationViewSelection(App.NavigationService.GetCurrentPage()!);
            }
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.AllowFocusOnInteraction = true;
            }
        }
        private void Flyout_Opening(object sender, object e)
        {
            if (sender is Flyout flyout &&
                flyout.Target is FrameworkElement element &&
                element.DataContext is Save save)
            {
                GameViewModel!.PrepareOverwrite(save);
            }
        }
    }
}
