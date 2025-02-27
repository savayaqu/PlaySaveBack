using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Api.Response;
using PSB.ContentDialogs;
using PSB.Converters;
using PSB.Models;
using PSB.Utils;

using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        [ObservableProperty] public partial ulong GameId {  get; set; }
        [ObservableProperty] public partial Game Game { get; set; }
        [ObservableProperty] public partial Library Library { get; set; }
        [ObservableProperty] public partial string LastPlayedText {  get; set; }
        [ObservableProperty] public partial string PlayedHoursText { get; set; }
        [ObservableProperty] public partial Boolean IsFavorite { get; set; }
        [ObservableProperty] public partial Boolean InLibrary { get; set; } = false;
        //[ObservableProperty] public partial Boolean ExeExists { get; set; } = false;
        [ObservableProperty] public partial string FilePath { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LaunchGameCommand))]
        public partial Boolean ExeExists { get; set; } = false;

        public string FavoriteIcon => IsFavorite ? "\uEB52" : "\uEB51";
        partial void OnIsFavoriteChanged(Boolean value)
        {
            OnPropertyChanged(nameof(FavoriteIcon));
        }
        public GameViewModel(ulong gameId)
        {
            GameId = gameId;
            _ = GetGameAsync();
        }
        [RelayCommand(CanExecute = nameof(ExeExists))]
        public async Task LaunchGame()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = FilePath,
                    UseShellExecute = true // Нужно для запуска GUI-приложений
                });

                Debug.WriteLine($"Игра запущена: {FilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при запуске игры: {ex.Message}");
            }
        }
        [RelayCommand]
        public async Task OpenGameSettings()
        {
            if (App.DialogService.XamlRoot == null)
            {
                Debug.WriteLine("XamlRoot is not set. Call SetXamlRoot first.");
                return;
            }
            var dialog = new GameSettingsContentDialog(Game);
            await App.DialogService.ShowDialogAsync(dialog);
        }
        
        
        [RelayCommand]
        public async Task ToggleFavorite()
        {
            var res = await FetchAsync(
                HttpMethod.Patch, $"library/game/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            IsFavorite = !IsFavorite;
        }
        [RelayCommand]
        public async Task AddToLibrary()
        {
            (var res, var body) = await FetchAsync<Library>(
                HttpMethod.Get, $"games/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if (!res.IsSuccessStatusCode)
                return;
            if ( body != null )
                Library = body;
            InLibrary = true;
        }
        public async Task GetGameAsync()
        {
            (var res, var body) = await FetchAsync<GameResponse>(
                HttpMethod.Get, $"games/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (!res.IsSuccessStatusCode)
                return;

            Game = body!.Game;
            FilePath = GameData.GetFilePath(Game);
            if(!string.IsNullOrEmpty(FilePath))
                ExeExists = true;

            if (body.Library != null)
            {
                Library = body.Library;
                LastPlayedText = Library.LastPlayedAt.HasValue
                    ? $"Последний запуск {GetDaysAgoText(Library.LastPlayedAt.Value)}"
                    : "Последний запуск: Никогда";

                PlayedHoursText = $"Сыграно {(Library.TimePlayed ?? 0) / 3600} часов";
                IsFavorite = Library.IsFavorite;
                InLibrary = true;
            }
            else
            {
                LastPlayedText = "Последний запуск: Никогда";
                PlayedHoursText = "Сыграно 0 часов";
                InLibrary = false;
            }
        }
       

        private string GetDaysAgoText(DateTime lastPlayed)
        {
            var daysAgo = (DateTime.UtcNow - lastPlayed).Days;
            return daysAgo switch
            {
                0 => "сегодня",
                1 => "вчера",
                _ => $"{daysAgo} дней назад"
            };
        }
    }
}
