using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Services;
using PSB.ViewModels;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public ProfileViewModel ProfileViewModel { get; set; }
        public AccountViewModel AccountViewModel { get; set; }

        private readonly NavigationService _navigationService;
        public Frame ContentFrameControl => ContentFrame;
        public NavigationView NavigationViewControl => NavView;
        public TextBlock HeaderTextBlock => HeaderText;
        public GeneralViewModel GeneralViewModel { get; set; }
        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();
            // Очистка локалки
            //ApplicationData.Current.LocalSettings.Values.Clear();
            ProfileViewModel = new ProfileViewModel();
            AccountViewModel = new AccountViewModel();
            GeneralViewModel = new GeneralViewModel();

            // Инициализируем сервисы
            _navigationService = new NavigationService(ContentFrame, NavView, HeaderText);
            NotificationService.Initialize(GlobalInfoBar, RootGrid);

            ContentFrame.Navigated += ContentFrame_Navigated;

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

    }
}