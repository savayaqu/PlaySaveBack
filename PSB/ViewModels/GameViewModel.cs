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
using PSB.Api.Response;
using PSB.Models;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        [ObservableProperty] public partial ulong GameId {  get; set; }
        [ObservableProperty] public partial SteamGame SteamGame {  get; set; }
        public GameViewModel(ulong gameId) 
        {
            GameId = gameId;
            _ = GetGameAsync();
        }
        public async Task GetGameAsync()
        {
            (var res, var body) = await FetchAsync<GameResponse>(
                HttpMethod.Get, $"games/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if (!res.IsSuccessStatusCode)
                return;
            // Сериализуем объект body в строку JSON для вывода в консоль
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true // Удобное форматирование
            });
            SteamGame = body.SteamGame;
            Debug.WriteLine("GameDescriptionHtml" + SteamGame.AboutTheGame);
        }
    }
}
