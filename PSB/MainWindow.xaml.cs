using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Services;
using PSB.Utils;
using PSB.ViewModels;
using Windows.Storage;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public ProfileViewModel ProfileViewModel { get; set; }

        private readonly NavigationService _navigationService;
        //private readonly LibraryService _libraryService;
        private readonly AuthService _authService;

        public Frame ContentFrameControl => ContentFrame;
        public NavigationView NavigationViewControl => NavView;
        public TextBlock HeaderTextBlock => HeaderText;
        public NavigationViewItem AuthNavControl => AuthNav;

        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();
            // Очистка локалки
            //ApplicationData.Current.LocalSettings.Values.Clear();
            ProfileViewModel = new ProfileViewModel();
            // Инициализируем сервисы
            _authService = new AuthService(ProfileViewModel, AuthNav);
            _navigationService = new NavigationService(ContentFrame, NavView, HeaderText);

            // Обновляем авторизацию при старте
            _ = _authService.UpdateAuthNavAsync();
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