using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PSB.Models;
using PSB.ViewModels;
using Windows.ApplicationModel.Contacts;
using Windows.Gaming.Input;


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
                App.NavigationService!.SyncNavigationViewSelection(App.NavigationService.GetCurrentPage());

                // Очищаем предыдущий контент
                GameContentGrid.Children.Clear();

                // Создаем новый контент в зависимости от типа
                if (GameViewModel.Game is Game game)
                {
                    CreateGameContent(game);
                }
                else if (GameViewModel.Game is SideGame sideGame)
                {
                    CreateSideGameContent(sideGame);
                }
            }
        }

        private void CreateGameContent(Game game)
        {
            // Создаем Image для обычной игры
            var image = new Image
            {
                Source = new BitmapImage(new Uri(game.Header!)),
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, -32, 0, 0)
            };

            GameContentGrid.Children.Add(image);
        }

        private void CreateSideGameContent(SideGame sideGame)
        {
            // Создаем TextBlock для SideGame
            var textBlock = new TextBlock
            {
                Text = sideGame.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };

            GameContentGrid.Children.Add(textBlock);
        }
    }
}
