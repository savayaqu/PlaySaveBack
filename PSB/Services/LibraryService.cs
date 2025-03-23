using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Models;
using PSB.Services;
using PSB.Utils.Game;
using PSB.ViewModels;
using System.Collections.Generic;
using System.Linq;

public class LibraryService
{
    private readonly NavigationView _navView;
    private readonly ProfileViewModel _profileViewModel;
    private readonly NavigationService _navigationService;
    private AutoSuggestBox _searchBox;
    private TextBlock _gamesCountTextBlock; // Для отображения количества игр
    private NavigationViewItem _libraryHeader; // Заголовок "БИБЛИОТЕКА"

    // Словарь для хранения состояния сворачивания категорий
    private readonly Dictionary<string, bool> _categoryCollapseState = new();

    public LibraryService(NavigationView navView, ProfileViewModel profileViewModel, NavigationService navigationService)
    {
        _navView = navView;
        _profileViewModel = profileViewModel;
        _navigationService = navigationService;

        // Инициализация AutoSuggestBox
        _searchBox = new AutoSuggestBox
        {
            PlaceholderText = "Поиск игр",
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
        };

        // Подписка на событие изменения текста
        _searchBox.TextChanged += OnSearchTextChanged;

        // Создаем заголовок "БИБЛИОТЕКА" с количеством игр
        _libraryHeader = new NavigationViewItem
        {
            Content = "БИБЛИОТЕКА",
            SelectsOnInvoked = false // Отключаем выбор элемента
        };

        // Создаем TextBlock для отображения количества игр
        _gamesCountTextBlock = new TextBlock
        {
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
            FontSize = 14
        };

        // Добавляем AutoSuggestBox и заголовок в NavigationView
        var searchItem = new NavigationViewItem
        {
            Content = _searchBox,
            SelectsOnInvoked = false // Отключаем выбор элемента
        };

        // Добавляем элементы в MenuItems
        _navView.MenuItems.Add(_libraryHeader);
        _navView.MenuItems.Add(searchItem);

        // Подписка на обновления библиотеки
        _profileViewModel.Libraries.CollectionChanged += (s, e) => UpdateLibraryMenu();
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            // Фильтрация игр по введенному тексту
            var searchText = sender.Text?.ToLower() ?? string.Empty;

            // Если поле поиска пустое, возвращаем все игры
            if (string.IsNullOrEmpty(searchText))
            {
                UpdateLibraryMenu(_profileViewModel.Libraries.ToList());
                return;
            }

            // Фильтруем игры по названию (включая сторонние игры)
            var filteredGames = _profileViewModel.Libraries
                .Where(game => (game.Game != null && game.Game.Name.ToLower().Contains(searchText)) ||
                               (game.SideGame != null && game.SideGame.Name.ToLower().Contains(searchText)))
                .ToList();

            // Обновление меню с учетом фильтрации
            UpdateLibraryMenu(filteredGames);
        }
    }

    private bool _isUpdatingLibrary = false;

    public void UpdateLibraryMenu(List<Library> filteredGames = null)
    {
        if (_isUpdatingLibrary) return;
        _isUpdatingLibrary = true;

        try
        {
            // Удаляем старые элементы библиотеки (игры и заголовки)
            var existingItems = _navView.MenuItems
                .OfType<NavigationViewItem>()
                .Where(item => item.Tag?.ToString()?.StartsWith("Game_") == true || item.Tag?.ToString()?.StartsWith("SideGame_") == true)
                .ToList();

            var existingHeaders = _navView.MenuItems
                .OfType<NavigationViewItem>()
                .Where(item => item.Content?.ToString()?.StartsWith("УСТАНОВЛЕНЫ") == true ||
                               item.Content?.ToString()?.StartsWith("ИЗБРАННОЕ") == true ||
                               item.Content?.ToString()?.StartsWith("БЕЗ КАТЕГОРИИ") == true ||
                               item.Content?.ToString()?.StartsWith("СТОРОННИЕ ИГРЫ") == true)
                .ToList();

            foreach (var item in existingItems) _navView.MenuItems.Remove(item);
            foreach (var header in existingHeaders) _navView.MenuItems.Remove(header);

            // Используем отфильтрованные игры, если они есть
            var gamesToDisplay = filteredGames ?? _profileViewModel.Libraries.ToList();

            // Обновляем заголовок "БИБЛИОТЕКА" с количеством игр
            _libraryHeader.Content = $"БИБЛИОТЕКА ({gamesToDisplay.Count})";

            // Разделяем игры по категориям
            var installedGames = gamesToDisplay
                .Where(game => game?.Game != null && PathDataManager<IGame>.GetFilePath(game.Game) != null)
                .ToList();

            var favoriteGames = gamesToDisplay
                .Where(game => game?.Game != null && game.IsFavorite && PathDataManager<IGame>.GetFilePath(game.Game) == null)
                .ToList();

            var uncategorizedGames = gamesToDisplay
                .Where(game => game?.Game != null && PathDataManager<IGame>.GetFilePath(game.Game) == null && !game.IsFavorite)
                .ToList();

            var sideGames = gamesToDisplay
                .Where(game => game?.SideGame != null)
                .ToList();

            // Функция для добавления заголовка и игр
            void AddCategory(string headerText, List<Library> games, bool isSideGame = false)
            {
                if (games.Count == 0) return;

                // Создаем заголовок с количеством игр
                var header = new NavigationViewItem
                {
                    Content = $"{headerText} ({games.Count})",
                    IsExpanded = _categoryCollapseState.TryGetValue(headerText, out var isExpanded) ? isExpanded : true,
                    SelectsOnInvoked = false // Отключаем выбор элемента
                };

                _navView.MenuItems.Add(header);

                // Добавляем игры, если категория развернута
                if (header.IsExpanded)
                {
                    foreach (var game in games)
                    {
                        NavigationViewItem gameItem;
                        if (game.Game != null && !isSideGame)
                        {
                            gameItem = new NavigationViewItem
                            {
                                Content = game.Game.Name,
                                Tag = $"Game_{game.Game.Id}|{game.Game.Name}",
                            };
                            if (PathDataManager<IGame>.GetFilePath(game.Game) != null)
                            {
                                var exeIcon = IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(game.Game));
                                if (exeIcon != null)
                                    gameItem.Icon = exeIcon;
                            }
                            else
                            {
                                // Если путь к исполняемому файлу отсутствует, используем стандартную иконку
                                gameItem.Icon = new FontIcon { Glyph = "\uE7FC" };
                            }
                        }
                        else if (game.SideGame != null && isSideGame)
                        {
                            gameItem = new NavigationViewItem
                            {
                                Content = game.SideGame.Name,
                                Tag = $"SideGame_{game.SideGame.Id}|{game.SideGame.Name}",
                            };
                            if (PathDataManager<IGame>.GetFilePath(game.SideGame) != null)
                            {
                                var exeIcon = IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(game.SideGame));
                                if (exeIcon != null)
                                    gameItem.Icon = exeIcon;
                            }
                            else
                            {
                                // Если путь к исполняемому файлу отсутствует, используем стандартную иконку
                                gameItem.Icon = new FontIcon { Glyph = "\uE7FC" };
                            }
                        }
                        else
                        {
                            continue;
                        }

                        // Добавляем игру в MenuItems заголовка
                        header.MenuItems.Add(gameItem);
                    }
                }
            }

            // Добавляем категории, если в них есть игры
            AddCategory("УСТАНОВЛЕНЫ", installedGames);
            AddCategory("ИЗБРАННОЕ", favoriteGames);
            AddCategory("СТОРОННИЕ ИГРЫ", sideGames, true); // Добавляем категорию для сторонних игр
            AddCategory("БЕЗ КАТЕГОРИИ", uncategorizedGames);

            // Синхронизируем выделение после обновления меню
            _navigationService.SyncNavigationViewSelection(_navigationService.GetCurrentPage());
        }
        finally
        {
            _isUpdatingLibrary = false;
        }
    }
}