using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PSB.Utils;
using PSB.ViewModels;
using Windows.Storage;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public ProfileViewModel ProfileViewModel { get; set; }

        public MainWindow()
        {
            Instance = this; // Сохраняем текущий экземпляр

            this.InitializeComponent();
            //ContentFrame.Navigate(typeof(Views.HomePage));
            ContentFrame.Navigated += ContentFrame_Navigated; // Подписываемся на событие Navigated
            ProfileViewModel = new ProfileViewModel();
            UpdateAuthNav();
            
        }
        private void UpdateLibraryMenu()
        {
            // Удаляем старые элементы библиотеки
            var existingLibraryItems = nvSample.MenuItems
                .OfType<NavigationViewItem>()
                .Where(item => item.Tag?.ToString()?.StartsWith("LibraryGame_") == true)
                .ToList();

            foreach (var item in existingLibraryItems)
            {
                nvSample.MenuItems.Remove(item);
            }

            // Добавляем игры
            foreach (var game in ProfileViewModel.Collections)
            {
                var gameItem = new NavigationViewItem
                {
                    Content = game.Game.Name,
                    Tag = $"LibraryGame_{game.Game.Id}"
                };
                //var icon = new BitmapIcon
                //{
                //    UriSource = new Uri($"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.Game.SteamId}/logo.png"),
                //    Height = 40,
                //    Width = 40,
                //    ShowAsMonochrome = false,


                //};
                //gameItem.Icon = new SymbolIcon(Symbol.World);
                nvSample.MenuItems.Add(gameItem);
            }
        }
        public void Nav(string pageTag)
        {
            Type? pageType = Type.GetType($"PSB.Views.{pageTag}");
            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
            else
            {
                // Можно добавить сообщение об ошибке или обработку ситуации
                ContentFrame.Content = new TextBlock { Text = "Page not found" };
            }
        }
        public void UpdateAuthNav()
        {
            UpdateLibraryMenu();

            if (AuthData.User != null && AuthData.Token != null)
            {
                // Загружаем коллекции пользователя
                this.Activated += async (s, e) =>
                {
                    await ProfileViewModel.LoadCollectionsAsync();
                    UpdateLibraryMenu();
                };
                // Подписываемся на изменения коллекции
                ProfileViewModel.Collections.CollectionChanged += (s, e) => UpdateLibraryMenu();
                // Если пользователь авторизован, меняем элемент навигации на профиль
                AuthNav.Tag = "ProfilePage";
                AuthNav.Content = AuthData.User.Nickname;
            }
            else
            {
                UpdateLibraryMenu();
                // Если пользователь не авторизован, возвращаемся к LoginPage
                AuthNav.Tag = "LoginPage";
                AuthNav.Content = "LoginPage";
            }
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                Nav(selectedItem.Tag.ToString());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        // Обработчик события Navigated для обновления заголовка и синхронизации NavigationView
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // TODO: можно сделать что-то типа русификации

            // Обновляем заголовок в зависимости от текущей страницы
            if (e.Content is Page page)
            {
                //HeaderText.Text = page.GetType().Name.Replace("Page", ""); // Убираем "Page" из имени
                HeaderText.Text = page.GetType().Name;
            }

            // Управляем видимостью кнопки "Назад"
            BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            // Синхронизируем выбранный элемент в NavigationView
            SyncNavigationViewSelection();
        }

        // Метод для синхронизации выбранного элемента в NavigationView
        private void SyncNavigationViewSelection()
        {
            if (ContentFrame.Content is Page page)
            {
                string pageName = page.GetType().Name; // Получаем имя текущей страницы

                // Ищем соответствующий NavigationViewItem
                foreach (var item in nvSample.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == pageName)
                    {
                        nvSample.SelectedItem = navItem;
                        break;
                    }
                }
            }
        }
    }
}