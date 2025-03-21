using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using PSB.Views;
using System.Linq;
using Microsoft.UI.Xaml.Media.Animation;
using PSB.Models;

namespace PSB.Services
{
    public class NavigationService
    {
        private readonly Frame _frame;
        private readonly NavigationView _navView;
        private readonly TextBlock _headerText;
        private readonly Dictionary<string, Type> _pages = new()
        {
            { "HomePage", typeof(HomePage) },
            { "CatalogPage", typeof(CatalogPage) },
            { "SettingsPage", typeof(SettingsPage) },
            { "ProfilePage", typeof(ProfilePage) },
            { "LoginPage", typeof(LoginPage) },
            { "SavePage", typeof(SavePage) },
        };
        public Page GetCurrentPage()
        {
            return _frame.Content as Page;
        }
        public NavigationService(Frame frame, NavigationView navView, TextBlock headerText)
        {
            _frame = frame;
            _navView = navView;
            _headerText = headerText;

            _frame.Navigated += OnNavigated;
            _navView.SelectionChanged += OnNavigationViewSelectionChanged;
        }

        public void Navigate(string pageTag)
        {
            if (string.IsNullOrEmpty(pageTag))
            {
                Debug.WriteLine("Page tag is null or empty.");
                return;
            }

            try
            {
                if (pageTag.StartsWith("Game_") || pageTag.StartsWith("SideGame_"))
                {
                    ulong gameId = ExtractGameId(pageTag);
                    string gameName = ExtractGameName(pageTag);
                    string type = pageTag.StartsWith("Game_") ? "Game" : "SideGame";

                    if (_frame.Content is GamePage currentGamePage && currentGamePage.GameViewModel?.GameId == gameId)
                    {
                        Debug.WriteLine("GamePage уже загружена для этой игры. Обновляем выделение в меню.");
                        _headerText.Text = gameName;
                        return;
                    }

                    var parameters = new GameNavigationParameters
                    {
                        Type = type,
                        GameId = gameId
                    };

                    _frame.Navigate(typeof(GamePage), parameters);
                    _headerText.Text = gameName;
                    return;
                }

                if (_pages.TryGetValue(pageTag, out var pageType))
                {
                    _frame.Navigate(pageType);
                    _headerText.Text = pageTag;
                }
                else
                {
                    Debug.WriteLine($"Page type not found for tag: {pageTag}");
                    _frame.Content = new TextBlock { Text = "Page not found" };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                _frame.Content = new TextBlock { Text = "Navigation error" };
            }
        }




        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                string pageTag = selectedItem.Tag.ToString();

                // Проверяем, что текущая страница уже не соответствует выбранному элементу
                if (_frame.Content?.GetType().Name == _pages.GetValueOrDefault(pageTag)?.Name)
                {
                    return;
                }

                Navigate(pageTag);
            }
        }



        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
            {
                page.Loaded += (s, _) =>
                {
                    if (page.Content.XamlRoot != null)
                    {
                        App.DialogService.SetXamlRoot(page.Content.XamlRoot);
                    }
                };

                // Не обновляем заголовок для GamePage, он обновится после загрузки данных
                if (page is not GamePage)
                {
                    _headerText.Text = page.GetType().Name;
                }

                if (_navView.SelectedItem != null && _navView.SelectedItem is NavigationViewItem selectedItem && selectedItem.Tag?.ToString() != page.GetType().Name)
                {
                    SyncNavigationViewSelection(page);
                }
            }
        }

        public void SyncNavigationViewSelection(Page page)
        {
            // Временно отключаем обработчик SelectionChanged
            _navView.SelectionChanged -= OnNavigationViewSelectionChanged;

            NavigationViewItem selectedItem = null;

            if (page == null)
            {
                // Обработка случая, когда page равен null
                Debug.WriteLine("Page is null. Cannot sync navigation view selection.");
                return;
            }

            // Обработка специальных страниц, таких как GamePage
            if (page is GamePage gamePage && gamePage.GameViewModel?.Game != null)
            {
                string gameTag = $"Game_{gamePage.GameViewModel.GameId}|{gamePage.GameViewModel.Game.Name}";
                selectedItem = FindNavigationViewItemByTag(gameTag);
            }
            else
            {
                // Обработка обычных страниц
                selectedItem = FindNavigationViewItemByTag(page.GetType().Name);
            }

            // Устанавливаем выбранный элемент, если он найден
            if (selectedItem != null)
            {
                _navView.SelectedItem = selectedItem;
            }

            // Включаем обработчик SelectionChanged обратно
            _navView.SelectionChanged += OnNavigationViewSelectionChanged;
        }

        private NavigationViewItem FindNavigationViewItemByTag(string tag)
        {
            return _navView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == tag);
        }

        private ulong ExtractGameId(string pageTag)
        {
            var parts = pageTag.Split('|');
            return Convert.ToUInt64(parts[0].Replace("Game_", ""));
        }

        private string ExtractGameName(string pageTag)
        {
            var parts = pageTag.Split('|', 2); // Ограничиваем Split, чтобы избежать лишних разбиений
            return parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "Unknown Game";
        }

    }
}
