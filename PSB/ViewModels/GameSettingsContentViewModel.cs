using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Drawing;
using PSB.Models;
using PSB.Utils;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Helpers;

namespace PSB.ViewModels
{
    public partial class GameSettingsContentViewModel : ObservableObject
    {
        [ObservableProperty] public partial string? SelectedFile { get; set; }
        [ObservableProperty] public partial string? SelectedSavesFolder { get; set; }
        [ObservableProperty] public partial string? GameName { get; set; }
        [ObservableProperty] public partial BitmapImage ExeImage { get; set; } 
        [ObservableProperty] public partial Icon ExeIcon { get; set; } 
        [ObservableProperty] public partial Game Game { get; set; }
        //TODO: Придумать что-нибудь с иконкой и картиной .exe
        public GameSettingsContentViewModel(Game game) 
        {
            Game = game;
            GameName = Game.Name;
            SelectedFile = GameData.GetFilePath(Game);
            SelectedSavesFolder = GameData.GetSavesFolderPath(Game);
            //ExeIcon = IconToBitmapImage.IconToBitmapImageAsync(IconFromExe.GetExeIcon(SelectedFile));
            ExeIcon = IconFromExe.GetExeIcon(SelectedFile);
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
                GameData.SetSavesFolderPath(Game, folder.Path);
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
                GameData.SetFilePath(Game, file.Path);
                SelectedFile = file.Path;
                Debug.WriteLine("Game name " + file.Name);
                Debug.WriteLine("Game DisplayName " + file.DisplayName);

                // Получаем иконку .exe-файла
                try
                {
                    var icon = IconFromExe.GetExeIcon(file.Path);
                    if (icon != null)
                    {
                        // Преобразуем иконку в BitmapImage для отображения
                        var bitmapImage = await IconToBitmapImage.IconToBitmapImageAsync(icon);
                        ExeImage = bitmapImage;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при получении иконки: {ex.Message}");
                }

                string? gameDirectory = Path.GetDirectoryName(file.Path);
                string? savesFolder = FindSavesFolder(gameDirectory);

                if (savesFolder != null)
                {
                    GameData.SetSavesFolderPath(Game, savesFolder);
                    SelectedSavesFolder = savesFolder;
                    Debug.WriteLine($"Папка Saves найдена: {savesFolder}");
                }
                else
                {
                    Debug.WriteLine("Папка Saves не найдена.");
                }
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
            // Логика удаления из библиотеки
        }

        [RelayCommand]
        private async Task RemoveAllSaves()
        {
            // Логика удаления всех сохранений
        }
    }
}