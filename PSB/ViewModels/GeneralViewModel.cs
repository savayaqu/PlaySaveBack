using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Utils;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace PSB.ViewModels
{
    public partial class GeneralViewModel : ObservableObject
    {
        [ObservableProperty] public partial string? PathToSave { get; set; } = SettingsData.PathToLocalSaves;
        [ObservableProperty] public partial bool DeleteSaveAfterSync { get; set; } = SettingsData.DeleteLocalSaveAfterSync;

        [RelayCommand]
        public void ToggleDeleteLocalSaveAfterSync()
        {
            DeleteSaveAfterSync = !DeleteSaveAfterSync;
            SettingsData.DeleteLocalSaveAfterSync = DeleteSaveAfterSync;
        }
        [RelayCommand]
        public async Task OpenSavesFolder()
        {
            if (string.IsNullOrWhiteSpace(PathToSave))
            {
                Debug.WriteLine("Путь не указан");
                return;
            }

            try
            {
                // Получаем папку как StorageFolder
                var folder = await StorageFolder.GetFolderFromPathAsync(PathToSave);

                // Открываем через Launcher
                bool success = await Launcher.LaunchFolderAsync(folder);

                if (!success)
                {
                    Debug.WriteLine("Не удалось открыть папку");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        [RelayCommand]
        public async Task ChooseSavesFolder()
        {
            // Create a folder picker
            FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.MainWindow;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                PathToSave = folder.Path;
                SettingsData.PathToLocalSaves = PathToSave;
            }

        }
    }
}
