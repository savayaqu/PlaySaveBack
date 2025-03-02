using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Utils;
using PSB.ViewModels;
using PSB.Views;
using PSB.Models;
using PSB.Services;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public ProfileViewModel ProfileViewModel { get; set; }

        private readonly NavigationService _navigationService;
        private readonly LibraryService _libraryService;
        private readonly AuthService _authService;

        public Frame ContentFrameControl => ContentFrame;
        public NavigationView NavigationViewControl => NavView;
        public TextBlock HeaderTextBlock => HeaderText;
        public NavigationViewItem AuthNavControl => AuthNav;

        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();

            ProfileViewModel = new ProfileViewModel();

            // Инициализируем сервисы
            _authService = new AuthService(ProfileViewModel, AuthNav);
            _navigationService = new NavigationService(ContentFrame, NavView, HeaderText);
            _libraryService = new LibraryService(NavView, ProfileViewModel, _navigationService);

            // Обновляем авторизацию при старте
            _ = _authService.UpdateAuthNavAsync();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }
    }
}