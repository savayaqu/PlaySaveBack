using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Models;
using PSB.Utils.Game;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PSB.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LocalSavesExists))]
        public partial ObservableCollection<GroupedSaves> LocalSaves { get; set; } = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LastPlayedGameExists))]
        public partial LastPlayedGame LastPlayedGame { get; set; }
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        public IconElement GameIcon { get; set; }
        public bool LastPlayedGameExists => LastPlayedGame != null;
        public bool LocalSavesExists => LocalSaves?.Any() == true;
        public HomeViewModel()
        {
            LoadLocalSaves();
            LastPlayedGame = LastPlayedGameManager.LoadLastPlayedGame();
            if (LastPlayedGame != null)
            {
                var icon = IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(LastPlayedGame.Game));
                if (icon != null)
                {
                    GameIcon = icon;
                }
                else
                {
                    GameIcon = new FontIcon { Glyph = "\uE7FC" };
                }
            }
        }
        public void LoadLocalSaves()
        {
            IsLoading = true;
            LocalSaves.Clear();

            try
            {
                var grouped = SavesDataManager<IGame>.GetAllLocalSaves();

                foreach (var group in grouped)
                {
                    var game = group.Key; // можно извлечь ID и преобразовать в читаемое имя, если нужно
                    var unsyncedSaves = group.Value;

                    if (unsyncedSaves.Any())
                    {
                        LocalSaves.Add(new GroupedSaves
                        {
                            Game = game,
                            Saves = unsyncedSaves
                        });
                    }
                }

                Debug.WriteLine($"Успешно загружено {LocalSaves.Count} групп");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке сохранений: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectGame(IGame game)
        {
            if (game != null)
            {
                string type = game.Type == "game" ? "Game" : "SideGame";
                string gameTag = $"{type}_{game.Id}|{game.Name}";
                App.NavigationService!.Navigate(gameTag);
            }
        }
    }
}
