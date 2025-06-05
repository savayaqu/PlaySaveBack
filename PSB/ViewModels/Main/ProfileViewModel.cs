using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Models;
using PSB.Utils;
using PSB.Utils.Game;
using PSB.Views.Profile;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using static PSB.Utils.Fetch;


namespace PSB.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        // Профиль и библиотека берутся из AuthData
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        public ObservableCollection<Library> Libraries => AuthData.Libraries;
        public ObservableCollection<RecentlyPlayed> GamesRecentlyPlayed = [];

        [ObservableProperty] public partial uint TotalPlayed { get; set; }
        public ProfileViewModel()
        {
            _ = LoadProfileAsync();
            _ = LoadLibraryAsync();
            _ = LoadStatistic();
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            await AuthData.LoadProfileAsync();
        }
        [RelayCommand]
        public async Task UpdateProfile()
        {
            var dialog = new UpdateProfileContentDialog();
            await App.DialogService!.ShowDialogAsync(dialog);
        }

        [RelayCommand]
        public async Task LoadLibraryAsync()
        {
            await AuthData.LoadLibraryAsync();
            OnPropertyChanged(nameof(Libraries)); // Уведомляем интерфейс об изменении
        }

        [RelayCommand]
        private void Logout()
        {
            _ = AuthData.ExitAndNavigate();
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
        [RelayCommand]
        public async Task LoadStatistic()
        {
            (var res, var body) = await FetchAsync<StatisticResponse>(HttpMethod.Get, "profile/statistic");
            if (res.IsSuccessStatusCode && body != null)
            {
                TotalPlayed = body.TotalPlayed;
                GamesRecentlyPlayed = [.. body.RecentlyPlayed];
                OnPropertyChanged(nameof(GamesRecentlyPlayed));

            }

        }
        [RelayCommand]
        public async Task AddSideGame()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".exe");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                (var res, var library) = await FetchAsync<Library>(
                   HttpMethod.Post, "sidegames",
                   body: new CreateSideGame(file.DisplayName),
                   serialize: true
               );

                // Проверка, что десериализация прошла успешно
                if (res.IsSuccessStatusCode && library != null)
                {
                    PathDataManager<IGame>.SetFilePath(library.SideGame!, file.Path);

                    string? gameDirectory = Path.GetDirectoryName(file.Path);
                    string? savesFolder = FindSaves.FindSavesFolder(gameDirectory);

                    if (savesFolder != null)
                    {
                        PathDataManager<IGame>.SetSavesFolderPath(library.SideGame!, savesFolder);
                        Debug.WriteLine($"Папка Saves найдена: {savesFolder}");
                    }
                    else
                    {
                        Debug.WriteLine("Папка Saves не найдена.");
                    }
                    GameDataManager.SaveGame(library.SideGame!);
                    // Обновление меню
                    Libraries.Add(library);
                    LibraryDataManager<IGame>.SaveLibrary(library.SideGame!, library);
                }
            }
        }
    }
}
