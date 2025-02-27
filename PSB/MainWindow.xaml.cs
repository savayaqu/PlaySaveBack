using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Utils;
using PSB.ViewModels;
using PSB.Views;

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
            App.DialogService.SetXamlRoot(this.Content.XamlRoot); // Устанавливаем XamlRoot для DialogService

            //ContentFrame.Navigate(typeof(Views.HomePage));
            ContentFrame.Navigated += ContentFrame_Navigated; // Подписываемся на событие Navigated
            ProfileViewModel = new ProfileViewModel();
            _ = UpdateAuthNavAsync();

        }
        private void UpdateLibraryMenu()
        {
            // Удаляем старые элементы библиотеки
            var existingLibraryItems = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .Where(item => item.Tag?.ToString()?.StartsWith("LibraryGame_") == true)
                .ToList();

            foreach (var item in existingLibraryItems)
            {
                NavView.MenuItems.Remove(item);
            }

            // Добавляем игры
            foreach (var game in ProfileViewModel.Library)
            {
                if (game?.Game == null) continue;

                var gameItem = new NavigationViewItem
                {
                    Content = game.Game.Name,
                    Tag = $"LibraryGame_{game.Game.Id}"
                };
                NavView.MenuItems.Add(gameItem);
            }
        }
        public void Nav(string pageTag)
        {
            if (string.IsNullOrEmpty(pageTag))
            {
                Debug.WriteLine("Page tag is null or empty.");
                return;
            }

            try
            {
                if (pageTag.StartsWith("LibraryGame_"))
                {
                    ulong gameId = Convert.ToUInt64(pageTag.Replace("LibraryGame_", ""));
                    var currentItem = NavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(item => item.Tag?.ToString() == pageTag);

                    if (currentItem != null)
                    {
                        ContentFrame.Navigate(typeof(GamePage), gameId);
                        HeaderText.Text = currentItem.Content.ToString();
                    }
                }
                else
                {
                    Type pageType = Type.GetType($"PSB.Views.{pageTag}");
                    if (pageType != null)
                    {
                        ContentFrame.Navigate(pageType);
                    }
                    else
                    {
                        Debug.WriteLine($"Page type not found for tag: {pageTag}");
                        ContentFrame.Content = new TextBlock { Text = "Page not found" };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                ContentFrame.Content = new TextBlock { Text = "Navigation error" };
            }
        }
        public async Task UpdateAuthNavAsync()
        {
            try
            {
                // Подписываемся на изменения коллекции
                ProfileViewModel.Library.CollectionChanged += (s, e) => UpdateLibraryMenu();

                if (AuthData.User != null && AuthData.Token != null)
                {
                    await ProfileViewModel.LoadLibraryAsync();
                    AuthNav.Tag = "ProfilePage";
                }
                else
                {
                    ProfileViewModel.Library.Clear();
                    AuthNav.Tag = "LoginPage";
                    AuthNav.Content = "LoginPage";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating auth navigation: {ex.Message}");
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
            if (e.Content is Page page)
            {
                page.Loaded += (s, _) =>
                {
                    if (page.Content.XamlRoot != null)
                    {
                        App.DialogService.SetXamlRoot(page.Content.XamlRoot);

                        Debug.WriteLine("XamlRoot успешно обновлен после загрузки страницы.");
                    }
                    else
                    {
                        Debug.WriteLine("Ошибка: XamlRoot остался null после загрузки.");
                    }
                };
                HeaderText.Text = page.GetType().Name;
                // Управляем видимостью кнопки "Назад"
                BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
                // Синхронизируем выбранный элемент в NavigationView
                SyncNavigationViewSelection();
            }
        }

        

        
        // Метод для синхронизации выбранного элемента в NavigationView
        private void SyncNavigationViewSelection()
        {
            if (ContentFrame.Content is Page page)
            {
                string pageName = page.GetType().Name;

                var selectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == pageName);

                if (selectedItem != null)
                {
                    NavView.SelectedItem = selectedItem;
                }
            }
        }
    }
}