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
            // Подписываемся на изменения коллекции
            ProfileViewModel.Libraries.CollectionChanged += (s, e) => UpdateLibraryMenu();
            _ = UpdateAuthNavAsync();
        }
        public void UpdateLibraryMenu()
        {
            // Удаляем старые элементы библиотеки (игры и заголовки)
            var existingItems = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .Where(item => item.Tag?.ToString()?.StartsWith("LibraryGame_") == true)
                .ToList();

            var existingHeaders = NavView.MenuItems
                .OfType<NavigationViewItemHeader>()
                .Where(header => header.Content?.ToString() is "УСТАНОВЛЕНЫ" or "ИЗБРАННОЕ" or "БЕЗ КАТЕГОРИИ")
                .ToList();

            foreach (var item in existingItems) NavView.MenuItems.Remove(item);
            foreach (var header in existingHeaders) NavView.MenuItems.Remove(header);

            // Разделяем игры по категориям
            var installedGames = ProfileViewModel.Libraries
                .Where(game => game?.Game != null && GameData.GetFilePath(game.Game) != null)
                .ToList();

            var favoriteGames = ProfileViewModel.Libraries
                .Where(game => game?.Game != null && game.IsFavorite && GameData.GetFilePath(game.Game) == null) // Исключаем установленные игры
                .ToList();

            var uncategorizedGames = ProfileViewModel.Libraries
                .Where(game => game?.Game != null && GameData.GetFilePath(game.Game) == null && !game.IsFavorite) // Исключаем установленные и избранные игры
                .ToList();

            // Функция для добавления заголовка и игр
            void AddCategory(string headerText, List<Library> games)
            {
                if (games.Count == 0) return;

                var header = new NavigationViewItemHeader { Content = headerText };
                NavView.MenuItems.Add(header);

                foreach (var game in games)
                {
                    var gameItem = new NavigationViewItem
                    {
                        Content = game.Game?.Name,
                        Tag = $"LibraryGame_{game.Game?.Id}",
                        Icon = new FontIcon { Glyph = "\uE7FC" }
                    };
                    NavView.MenuItems.Add(gameItem);
                }
            }

            // Добавляем категории, если в них есть игры
            AddCategory("УСТАНОВЛЕНЫ", installedGames);
            AddCategory("ИЗБРАННОЕ", favoriteGames);
            AddCategory("БЕЗ КАТЕГОРИИ", uncategorizedGames);

            // Синхронизируем выбранный элемент
            SyncNavigationViewSelection();
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

                    ContentFrame.Navigate(typeof(GamePage), gameId);
                    HeaderText.Text = currentItem?.Content?.ToString() ?? $"Игра {gameId}";
                    return;
                }

                if (pageTag.StartsWith("Game_"))
                {
                    var parts = pageTag.Split('|');
                    ulong gameId = Convert.ToUInt64(parts[0].Replace("Game_", ""));
                    string gameName = parts.Length > 1 ? parts[1] : "Unknown Game";
                    ContentFrame.Navigate(typeof(GamePage), gameId);
                    HeaderText.Text = gameName; // Используем переданное имя
                    return;
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

                if (AuthData.User != null && AuthData.Token != null)
                {
                    await ProfileViewModel.LoadLibraryAsync();
                    AuthNav.Tag = "ProfilePage";
                }
                else
                {
                    ProfileViewModel.Libraries.Clear();
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
                // Обновляем заголовок после возврата
                if (ContentFrame.Content is Page page)
                {
                    if (page is GamePage gamePage)
                    {
                        HeaderText.Text = gamePage.Name;
                        Debug.WriteLine("HeaderText" + HeaderText.Text);
                    }
                    else
                    {
                        HeaderText.Text = page.GetType().Name;
                        Debug.WriteLine("HeaderText" + HeaderText.Text);

                    }
                }
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
                // Управляем видимостью кнопки "Назад"
                BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

                // Синхронизируем выбранный элемент в NavigationView
                SyncNavigationViewSelection();
            }
        }

        // Метод для синхронизации выбранного элемента в NavigationView
        private void SyncNavigationViewSelection()
        {
            // Проверяем, что содержимое ContentFrame является страницей
            if (ContentFrame.Content is Page page)
            {
                string pageName = page.GetType().Name;
                Debug.WriteLine("pageName: " + pageName);

                NavigationViewItem selectedItem = null;

                if (page is GamePage gamePage)
                {
                    // Если это GamePage, получаем GameId из параметров навигации
                    if (gamePage.GameId != 0)
                    {
                        string gameTag = $"LibraryGame_{gamePage.GameId}";
                        selectedItem = NavView.MenuItems
                            .OfType<NavigationViewItem>()
                            .FirstOrDefault(item => item.Tag?.ToString() == gameTag);
                    }
                }
                else
                {
                    // Для других страниц ищем по имени типа страницы
                    selectedItem = NavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(item => item.Tag?.ToString() == pageName);
                }

                if (selectedItem != null)
                {
                    NavView.SelectedItem = selectedItem;
                }
                else
                {
                    // Если элемент не найден, сбрасываем выделение
                    NavView.SelectedItem = null;
                }
            }
            else
            {
                // Если содержимое не является страницей, сбрасываем выделение
                NavView.SelectedItem = null;
            }
        }
    }
}