using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.ViewModels;
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
            Loaded += (s, e) =>
            {
                if (DataContext is GameViewModel viewModel)
                {
                    viewModel.GameLoaded += () => App.NavigationService.SyncNavigationViewSelection(this);
                }
            };
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ulong gameId)
            {
                // ≈сли GameViewModel уже существует, пропускаем создание нового
                if (GameViewModel == null || GameViewModel.GameId != gameId)
                {
                    GameViewModel = new GameViewModel(gameId);
                    DataContext = GameViewModel;
                }
            }
        }

    }
}
