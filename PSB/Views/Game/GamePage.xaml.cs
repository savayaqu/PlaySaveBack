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
using Windows.UI;


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
            // Основное изображение игры (фон)
            var image = new Image
            {
                Source = new BitmapImage(new Uri(game.Header!)),
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Логотип игры (смещен влево и немного вниз)
            var logo = new Image
            {
                Source = new BitmapImage(new Uri(game.LogoImg!)),
                Stretch = Stretch.Uniform, 
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 80, 0, 0), 
                Width = 300, 
            };

            GameContentGrid.Children.Add(image);
            GameContentGrid.Children.Add(logo);
        }

        private void CreateSideGameContent(SideGame sideGame)
        {
            // Создаем TextBlock для SideGame
            var textBlock = new TextBlock
            {
                Text = sideGame.Name,
                Padding = new Thickness(24, 0, 0, 100),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Style = (Style)Application.Current.Resources["DisplayTextBlockStyle"],
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            GameContentGrid.Background = (Brush)Application.Current.Resources["AcrylicBackgroundFillColorDefaultBrush"];
            GameContentGrid.Children.Add(textBlock);
        }
    }
}
