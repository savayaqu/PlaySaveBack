using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PSB.Utils;
using PSB.ViewModels;
using PSB.Views;
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
            _ = UpdateAuthNavAsync();
            
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
                if(game != null)
                {

                }
                var gameItem = new NavigationViewItem
                {
                    Content = game?.Game?.Name,
                    Tag = $"LibraryGame_{game?.Game?.Id}"
                };
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
            else if(pageTag.StartsWith("LibraryGame_"))
            {
                Debug.WriteLine("pageTag" + pageTag);
                ulong gameId = Convert.ToUInt64( pageTag.Replace("LibraryGame_", ""));
                Debug.WriteLine("gameId" + gameId);
                ContentFrame.Navigate(Type.GetType("PSB.Views.GamePage"), gameId);
            }
            else
            {
                // Можно добавить сообщение об ошибке или обработку ситуации
                ContentFrame.Content = new TextBlock { Text = "Page not found" };
            }
        }
        public void GameNav(string pageTag, string gameId)
        {
            Type? pageType = Type.GetType($"PSB.Views.{pageTag}");
            if (pageType != null)
            {
                new GamePage();
                ContentFrame.Navigate(pageType);
            }
        }
        public async Task UpdateAuthNavAsync()
        {
            // Подписываемся на изменения коллекции
            ProfileViewModel.Collections.CollectionChanged += (s, e) => UpdateLibraryMenu();

            if (AuthData.User != null && AuthData.Token != null)
            {
                await ProfileViewModel.LoadCollectionsAsync();
                // Если пользователь авторизован, меняем элемент навигации на профиль
                AuthNav.Tag = "ProfilePage";
                AuthNav.Content = AuthData.User.Nickname;
            }
            else
            {
                ProfileViewModel.Collections.Clear();
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