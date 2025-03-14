using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Helpers;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using PSB.ViewModels;
using System.Collections.Generic;
using System.Linq;

public class LibraryService
{
    private readonly NavigationView _navView;
    private readonly ProfileViewModel _profileViewModel;
    private readonly NavigationService _navigationService;

    public LibraryService(NavigationView navView, ProfileViewModel profileViewModel, NavigationService navigationService)
    {
        _navView = navView;
        _profileViewModel = profileViewModel;
        _navigationService = navigationService;

        // Подписка на обновления библиотеки
        _profileViewModel.Libraries.CollectionChanged += (s, e) => UpdateLibraryMenu();
    }

    private bool _isUpdatingLibrary = false;

    public void UpdateLibraryMenu()
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
                .OfType<NavigationViewItemHeader>()
                .Where(header => header.Content?.ToString() is "УСТАНОВЛЕНЫ" or "ИЗБРАННОЕ" or "БЕЗ КАТЕГОРИИ" or "СТОРОННИЕ ИГРЫ")
                .ToList();

            foreach (var item in existingItems) _navView.MenuItems.Remove(item);
            foreach (var header in existingHeaders) _navView.MenuItems.Remove(header);

            // Разделяем игры по категориям
            var installedGames = _profileViewModel.Libraries
                .Where(game => game?.Game != null && GameData.GetFilePath(game.Game) != null)
                .ToList();

            var favoriteGames = _profileViewModel.Libraries
                .Where(game => game?.Game != null && game.IsFavorite && GameData.GetFilePath(game.Game) == null)
                .ToList();

            var uncategorizedGames = _profileViewModel.Libraries
                .Where(game => game?.Game != null && GameData.GetFilePath(game.Game) == null && !game.IsFavorite)
                .ToList();

            var sideGames = _profileViewModel.Libraries
                .Where(game => game?.SideGame != null)
                .ToList();

            // Функция для добавления заголовка и игр
            void AddCategory(string headerText, List<Library> games, bool isSideGame = false)
            {
                if (games.Count == 0) return;

                var header = new NavigationViewItemHeader { Content = headerText };
                _navView.MenuItems.Add(header);

                foreach (var game in games)
                {
                    if (game.Game != null && !isSideGame)
                    {
                        var gameItem = new NavigationViewItem
                        {
                            Content = game.Game.Name,
                            Tag = $"Game_{game.Game.Id}|{game.Game.Name}",
                        };
                        if (GameData.GetFilePath(game.Game) != null)
                        {
                            var exeIcon = IconFromExe.GetIconElement(GameData.GetFilePath(game.Game));
                            if (exeIcon != null)
                                gameItem.Icon = exeIcon;
                        }
                        else
                        {
                            // Если путь к исполняемому файлу отсутствует, используем стандартную иконку
                            gameItem.Icon = new FontIcon { Glyph = "\uE7FC" };
                        }
                        _navView.MenuItems.Add(gameItem);
                    }
                    else if (game.SideGame != null && isSideGame)
                    {
                        var gameItem = new NavigationViewItem
                        {
                            Content = game.SideGame.Name,
                            Tag = $"SideGame_{game.SideGame.Id}|{game.SideGame.Name}",
                            Icon = new FontIcon { Glyph = "\uE7FC" }, // Стандартная иконка для сторонних игр
                        };
                        _navView.MenuItems.Add(gameItem);
                    }
                }
            }

            // Добавляем категории, если в них есть игры
            AddCategory("УСТАНОВЛЕНЫ", installedGames);
            AddCategory("ИЗБРАННОЕ", favoriteGames);
            AddCategory("БЕЗ КАТЕГОРИИ", uncategorizedGames);
            AddCategory("СТОРОННИЕ ИГРЫ", sideGames, true); // Добавляем категорию для сторонних игр

            // Синхронизируем выделение после обновления меню
            _navigationService.SyncNavigationViewSelection(_navigationService.GetCurrentPage());
        }
        finally
        {
            _isUpdatingLibrary = false;
        }
    }
}