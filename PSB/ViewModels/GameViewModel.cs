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
using PSB.Converters;
using PSB.Models;
using PSB.Utils;

using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        [ObservableProperty] public partial ulong GameId {  get; set; }
        [ObservableProperty] public partial SteamGame? SteamGame {  get; set; }
        [ObservableProperty] public partial RichTextBlock GameDescriptionXaml {  get; set; }
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
            SteamGame = body!.SteamGame;
            if (SteamGame?.AboutTheGame != null)
            {
                string htmlContent = SteamGame.AboutTheGame.Trim();
                if (!htmlContent.StartsWith("<"))
                {
                    htmlContent = $"<p>{htmlContent}</p>";
                }
                GameDescriptionXaml = HtmlToXamlConverter.ConvertHtmlToXaml(htmlContent);
            }
                
        }
    }
}
