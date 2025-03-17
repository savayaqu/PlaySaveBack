using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Models;
using PSB.Utils;
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class GameSettingsContentViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance?.ProfileViewModel!;
        public GameViewModel GameViewModel { get; set; } = GameViewModel.Instance!;
        [ObservableProperty] public partial string? SelectedFile { get; set; }
        [ObservableProperty] public partial string? SelectedSavesFolder { get; set; }
        [ObservableProperty] public partial string? GameName { get; set; }

        [ObservableProperty] public partial IGame Game { get; set; }
        //TODO: Придумать что-нибудь с иконкой и картиной .exe
        public GameSettingsContentViewModel(IGame iGame)
        {
            Game = iGame;
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
            }
        }



        [RelayCommand]
        private async Task ChooseFile()
        {
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
                string? savesFolder = FindSavesFolder(gameDirectory);

                if (savesFolder != null)
                {
                    PathDataManager<IGame>.SetSavesFolderPath(Game, savesFolder);
                    SelectedSavesFolder = savesFolder;
                    Debug.WriteLine($"Папка Saves найдена: {savesFolder}");
                }
                else
                {
                    Debug.WriteLine("Папка Saves не найдена.");
                }

                GameViewModel.ExeExists = true;
                GameViewModel.FilePath = SelectedFile;

                // Вызываем обновление меню
                App.LibraryService.UpdateLibraryMenu();
            }
        }



        /// <summary>
        /// Ищет папку Saves в указанной директории, на уровень выше и в подпапках.
        /// </summary>
        private string? FindSavesFolder(string? startDirectory)
        {
            if (string.IsNullOrEmpty(startDirectory))
                return null;

            string? currentDirectory = startDirectory;

            for (int i = 0; i < 2; i++) // Проверяем текущий уровень и на уровень выше
            {
                string? savesFolder = FindSavesInDirectory(currentDirectory);
                if (savesFolder != null)
                    return savesFolder;

                // Поднимаемся на уровень выше
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (currentDirectory == null)
                    break;
            }

            return null;
        }

        /// <summary>
        /// Проверяет наличие папки Saves в указанной директории, включая вложенные папки.
        /// </summary>
        private string? FindSavesInDirectory(string directory)
        {
            try
            {
                var directories = Directory.GetDirectories(directory);
                foreach (var dir in directories)
                {
                    if (string.Equals(Path.GetFileName(dir), "saves", StringComparison.OrdinalIgnoreCase))
                        return dir;

                    // Рекурсивный поиск внутри вложенных папок
                    string? found = FindSavesInDirectory(dir);
                    if (found != null)
                        return found;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine($"Нет доступа к: {directory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при поиске папки: {ex.Message}");
            }

            return null;
        }





        [RelayCommand]
        private async Task OpenSaves()
        {
            // Логика открытия сохранений
        }

        [RelayCommand]
        private async Task RemoveFromLibrary()
        {
            var res = await FetchAsync(
                HttpMethod.Delete, $"library/game/{Game.Id}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (res.IsSuccessStatusCode)
            {
                //TODO: очистка всего для игры
                //GameData.RemoveGameData(Game);

                // Удаляем игру из библиотеки
                var libraryItem = ProfileViewModel.Libraries.FirstOrDefault(l => l.Game.Id == Game.Id);
                if (libraryItem != null)
                    ProfileViewModel.Libraries.Remove(libraryItem);

                Debug.WriteLine("Попытка закрыть диалог...");
                App.DialogService.HideDialog();
                GameViewModel.InLibrary = false;
            }
        }



        [RelayCommand]
        private async Task RemoveAllSaves()
        {
            // Логика удаления всех сохранений
        }
    }
}