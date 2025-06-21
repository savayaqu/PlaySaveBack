using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Services;
using PSB.ViewModels;
using PSB.Views;
using Windows.Graphics;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public ProfileViewModel ProfileViewModel { get; set; }
        public AccountViewModel AccountViewModel { get; set; }

        public Frame ContentFrameControl => ContentFrame;
        public NavigationView NavigationViewControl => NavView;
        public TextBlock HeaderTextBlock => HeaderText;
        public GeneralViewModel GeneralViewModel { get; set; }
        public CatalogViewModel CatalogViewModel { get; set; }
        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();
            // Очистка локалки
            //ApplicationData.Current.LocalSettings.Values.Clear();
            ProfileViewModel = new ProfileViewModel();
            AccountViewModel = new AccountViewModel();
            GeneralViewModel = new GeneralViewModel();
            CatalogViewModel = new CatalogViewModel();

            // Инициализируем сервисы
            NotificationService.Initialize(GlobalInfoBar, RootGrid);
            ContentFrame.Navigated += ContentFrame_Navigated;

            // Установка минизмального значения окна
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 550));
            // Устанавливаем иконку
            AppWindow.SetIcon("Assets/Logo.ico");
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = 1100;
            presenter.PreferredMinimumHeight = 550;

            AppWindow.SetPresenter(presenter);

            // Центруем приложение
            var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
            if (area == null) return;
            AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (ContentFrame.Content is CatalogPage catalogPage)
            {
                catalogPage.CatalogViewModel.Name = sender.Text;
                catalogPage.CatalogViewModel.LoadGamesCommand.Execute(null);
            }
            await CatalogViewModel.LoadGamesAsync();
        }
    }
}