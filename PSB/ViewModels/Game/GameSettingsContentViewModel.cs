using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class GameSettingsContentViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance!.ProfileViewModel!;
        public GameViewModel GameViewModel { get; set; }
        [ObservableProperty] public partial string? SelectedFile { get; set; }
        [ObservableProperty] public partial string? SelectedSavesFolder { get; set; }
        [ObservableProperty] public partial string? GameName { get; set; }

        [ObservableProperty] public partial IGame Game { get; set; }
        public GameSettingsContentViewModel(IGame iGame, GameViewModel gameViewModel)
        {
            Game = iGame;
            GameViewModel = gameViewModel;
            GameName = Game.Name;
            SelectedFile = PathDataManager<IGame>.GetFilePath(Game);
            SelectedSavesFolder = PathDataManager<IGame>.GetSavesFolderPath(Game);
        }
        [RelayCommand]
        private async Task ChooseFolderSaves()
        {
            var openPicker = new Windows.Storage.Pickers.FolderPicker();

            // Получаем HWND текущего окна
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Проверяем, есть ли сохранённый путь
            if (!string.IsNullOrEmpty(SelectedFile))
            {
                string? parentFolderPath = Path.GetDirectoryName(SelectedFile);
                if (!string.IsNullOrEmpty(parentFolderPath))
                {
                    // Используем FutureAccessList, если путь ранее выбирался
                    openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                }
            }
            else
            {
                openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            }

            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                PathDataManager<IGame>.SetSavesFolderPath(Game, folder.Path);
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                SelectedSavesFolder = folder.Path;
                GameViewModel.FolderPath = SelectedSavesFolder;
                PathDataManager<IGame>.SetSavesFolderPath(Game, SelectedSavesFolder);
            }
        }



        [RelayCommand]
        private async Task ChooseFile()
        {
            if(SelectedFile != string.Empty)
            {
                SelectedSavesFolder = null;
                GameViewModel.FolderPath = null;
            }
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".exe");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PathDataManager<IGame>.SetFilePath(Game, file.Path);
                SelectedFile = file.Path;
                Debug.WriteLine("Game name " + file.Name);
                Debug.WriteLine("Game DisplayName " + file.DisplayName);

                string? gameDirectory = Path.GetDirectoryName(file.Path);
                string? savesFolder = FindSaves.FindSavesFolder(gameDirectory);

                if (savesFolder != null)
                {
                    PathDataManager<IGame>.SetSavesFolderPath(Game, savesFolder);
                    SelectedSavesFolder = savesFolder;
                    GameViewModel.FolderPath = SelectedSavesFolder;
                    Debug.WriteLine($"Папка Saves найдена: {savesFolder}");
                }
                else
                {
                    Debug.WriteLine("Папка Saves не найдена.");
                }

                GameViewModel.ExeExists = true;
                GameViewModel.FilePath = SelectedFile;
                // Вызываем обновление меню
                App.LibraryService!.UpdateLibraryMenu();
            }
        }

        [RelayCommand]
        private async Task OpenSaves()
        {
            // Логика открытия сохранений
        }

        [RelayCommand]
        private async Task RemoveFromLibrary()
        {
            if (Game.Type == "game")
            {
                var res = await FetchAsync(HttpMethod.Delete, $"library/game/{Game.Id}");

                if (res.IsSuccessStatusCode)
                {
                    // Находим элемент библиотеки, проверяя на null как сам элемент, так и его свойство Game
                    var libraryItem = ProfileViewModel.Libraries.FirstOrDefault(l => l?.Game?.Id == Game.Id);

                    if (libraryItem != null)
                    {
                        ProfileViewModel.Libraries.Remove(libraryItem);
                        Debug.WriteLine("Попытка закрыть диалог...");
                        App.DialogService!.HideDialog();
                        GameViewModel.InLibrary = false;
                    }
                    else
                    {
                        Debug.WriteLine("Элемент библиотеки не найден.");
                    }
                }
            }
            if (Game.Type == "sidegame")
            {
                var res = await FetchAsync(HttpMethod.Delete, $"library/sidegame/{Game.Id}");

                if (res.IsSuccessStatusCode)
                {
                    // Находим элемент библиотеки, проверяя на null как сам элемент, так и его свойство Game
                    var libraryItem = ProfileViewModel.Libraries.FirstOrDefault(l => l?.SideGame?.Id == Game.Id);

                    if (libraryItem != null)
                    {
                        ProfileViewModel.Libraries.Remove(libraryItem);
                        Debug.WriteLine("Попытка закрыть диалог...");
                        App.DialogService!.HideDialog();
                        GameViewModel.InLibrary = false;
                    }
                    else
                    {
                        Debug.WriteLine("Элемент библиотеки не найден.");
                    }
                    App.NavigationService!.Navigate("ProfilePage");
                }
            }

        }


        [RelayCommand]
        private async Task RemoveAllSaves()
        {
            // Логика удаления всех сохранений
        }
    }
}