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
        [ObservableProperty] public partial BitmapImage GameHeader {  get; set; }
        [ObservableProperty] public partial BitmapImage GameLogo {  get; set; }
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
            //GameHeader = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/library_hero.jpg";
            //GameLogo = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/logo.png";
            //GameHeader = LoadImage($"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/library_hero.jpg")?.UriSource?.ToString();
            //GameLogo = LoadImage($"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/logo.png")?.UriSource?.ToString();
            //GameLogo = new BitmapImage(new Uri($"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/logo.png"));
            //GameHeader = new BitmapImage(new Uri($"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamGame.SteamAppId}/library_hero.jpg"));
            //var image = new Image
            //{
            //    Source = new BitmapImage(new Uri(imgSrc)),
            //    //Margin = new Microsoft.UI.Xaml.Thickness(5) // Пространство вокруг изображения
            //};
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
        private BitmapImage LoadImage(string url)
        {
            Debug.WriteLine($"Зашел");

            try
            {
                return new BitmapImage(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                return null;
            }
        }
    }
}
