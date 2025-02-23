using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Api.Response;
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
