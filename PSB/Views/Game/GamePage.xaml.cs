using Microsoft.UI.Xaml.Controls;
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
                // ���� GameViewModel ��� ����������, ���������� �������� ������
                if (GameViewModel == null || GameViewModel.GameId != parameters.GameId)
                {
                    GameViewModel = new GameViewModel(parameters.GameId, parameters.Type);
                    DataContext = GameViewModel;

                    // ������������� �� ������� �������� ������
                    GameViewModel.GameLoaded += OnGameLoaded;
                }
            }
        }

        private void OnGameLoaded()
        {
            // ��������� ��������� ����� �������� ������
            if (GameViewModel?.Game != null)
            {
                App.MainWindow.HeaderTextBlock.Text = GameViewModel.Game.Name;
                App.NavigationService.SyncNavigationViewSelection(App.NavigationService.GetCurrentPage());
            }
        }
    }
}
