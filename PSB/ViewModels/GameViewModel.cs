using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.ContentDialogs;
using PSB.Models;
using PSB.Utils;
using System.IO;
using System.IO.Compression;
using static PSB.Utils.Fetch;
using System.Net.Http.Headers;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance?.ProfileViewModel!;
        public static GameViewModel? Instance { get; private set; }

        [ObservableProperty] public partial ulong GameId {  get; set; }
        [ObservableProperty] public partial Game Game { get; set; }
        [ObservableProperty] public partial Library Library { get; set; }
        [ObservableProperty] public partial string LastPlayedText {  get; set; }
        [ObservableProperty] public partial string PlayedHoursText { get; set; }
        [ObservableProperty] public partial Boolean IsFavorite { get; set; }
        [ObservableProperty] public partial Boolean InLibrary { get; set; } = false;
        [ObservableProperty] public partial string FilePath { get; set; }
        [ObservableProperty] public partial InfoBar SuccessInfoBar { get; set; }
        [ObservableProperty] public partial Boolean IsUploading { get; set; }
        [ObservableProperty] public partial ObservableCollection<Save>? Saves { get; set; } = new ObservableCollection<Save>();
        public event Action? GameLoaded;

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

            Instance = this;
            GameId = gameId;

            SuccessInfoBar = new InfoBar
            {
                Title = "",
                Message = "",
                Severity = InfoBarSeverity.Informational,
                IsOpen = false
            };
            //TODO: динамически не меняется сохранение, время, запуск.

            _ = GetGameAsync(false).ContinueWith(_ =>
            {
                GameLoaded?.Invoke();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }
        [RelayCommand(CanExecute = nameof(ExeExists))]
        public async Task LaunchGame()
        {
            try
            {
                Process gameProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = FilePath,
                        UseShellExecute = true
                    }
                };
                gameProcess.Start();
                Debug.WriteLine($"Игра запущена: {FilePath}");

                DateTime startTime = DateTime.Now;

                // Ожидание завершения процесса
                await Task.Run(() => gameProcess.WaitForExit());

                DateTime endTime = DateTime.Now;
                DateTime trimmedEndTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second);

                // Запись времени игры
                TimeSpan playTime = endTime - startTime;
                uint secondsPlayed = (uint)playTime.TotalSeconds;

                // Обновляем локальные данные
                Library.TimePlayed = (Library.TimePlayed ?? 0) + secondsPlayed;
                Library.LastPlayedAt = endTime;

                try
                {
                    // Отправляем данные на сервер
                    var res = await FetchAsync(
                        HttpMethod.Patch, $"library/game/{GameId}/update",
                        isFetch => Debug.WriteLine("isFetch " + isFetch),
                        error => Debug.WriteLine("error " + error),
                        new UpdateLibraryGameRequest(Library.TimePlayed, endTime.ToString("yyyy-MM-dd HH:mm:ss")),
                        serialize: true
                    );
                    GameData.SaveGameData(new GameResponse
                    {
                        Game = Game,
                        Library = Library,
                        Saves = Saves?.ToList()
                    });
                    GameLoaded?.Invoke();
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine("Ошибка соединения: " + ex.Message);
                }
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

            if (!res.IsSuccessStatusCode)
                return;

            IsFavorite = !IsFavorite;
            // Обновляем библиотеку
            var libraryItem = ProfileViewModel.Libraries.FirstOrDefault(l => l.Game?.Id == GameId);
            if (libraryItem != null)
            {
                libraryItem.IsFavorite = IsFavorite;
            }
            if (App.LibraryService != null)
            {
                App.LibraryService.UpdateLibraryMenu();
            }
        }
        public async Task<string> ZipFolder(string folderPath, string zipFilePath)
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath); // Удаляем существующий ZIP-файл, если он есть
            }

            ZipFile.CreateFromDirectory(folderPath, zipFilePath); // Создаём ZIP-архив
            return zipFilePath;
        }
        public async Task<bool> UploadFile(string filePath)
        {
            try
            {
                // Проверяем, существует ли файл
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine("Файл не найден.");
                    return false;
                }
                // Начинаем загрузку
                IsUploading = true;
                // Читаем файл в массив байтов
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                // Создаем MultipartFormDataContent
                var content = new MultipartFormDataContent();

                // Добавляем файл
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
                content.Add(fileContent, "file", Path.GetFileName(filePath));

                //TODO: предлагать на выбор

                // Добавляем текстовые поля
                content.Add(new StringContent("v1"), "version"); // Версия
                content.Add(new StringContent(Convert.ToString(GameId)), "game_id"); // ID игры

                // Отправляем запрос
                var res = await FetchAsync(
                    HttpMethod.Post,
                    "google-drive/upload",
                    body: content
                );
                
                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Файл успешно загружен на сервер.");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Ошибка при загрузке файла: {res.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
            finally { IsUploading = false; } 
        }

        [RelayCommand]
        public async Task UploadSave()
        {
            string zipFilePath = string.Empty;
            try
            {
                // Шаг 1: Сжимаем папку в ZIP
                zipFilePath = Path.Combine(Path.GetTempPath(), "saves.zip");
                Debug.WriteLine($"Временный файл: {zipFilePath}");
                await ZipFolder(GameData.GetSavesFolderPath(Game)!, zipFilePath);

                // Шаг 2: Отправляем ZIP-файл на сервер
                bool uploadSuccess = await UploadFile(zipFilePath);

                if (uploadSuccess)
                {
                    _ = GetGameAsync(true);
                    OnPropertyChanged(nameof(Saves)); // Дополнительно уведомляем об изменении

                    // Показываем уведомление об успехе
                    SuccessInfoBar.Title = "Успешно";
                    SuccessInfoBar.Message = "Сохранения успешно загружены на сервер.";
                    SuccessInfoBar.Severity = InfoBarSeverity.Success;
                    SuccessInfoBar.IsOpen = true;
                }
                else
                {
                    // Показываем уведомление об ошибке
                    SuccessInfoBar.Title = "Ошибка";
                    SuccessInfoBar.Message = "Не удалось загрузить сохранения на сервер.";
                    SuccessInfoBar.Severity = InfoBarSeverity.Error;
                    SuccessInfoBar.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");

                // Показываем уведомление об ошибке
                SuccessInfoBar.Title = "Ошибка";
                SuccessInfoBar.Message = $"Произошла ошибка: {ex.Message}";
                SuccessInfoBar.Severity = InfoBarSeverity.Error;
                SuccessInfoBar.IsOpen = true;
            }
            finally
            {
                // Удаляем временный ZIP-файл
                if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        [RelayCommand]
        public async Task AddToLibrary()
        {
            (var res, var body) = await FetchAsync<Library>(
                HttpMethod.Post, $"library/game/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if (!res.IsSuccessStatusCode)
                return;

            if (body != null)
            {
                Library = body;
                InLibrary = true;
                ProfileViewModel.Libraries.Add(Library);

                // Обновляем кэш
                GameData.SaveGameData(new GameResponse
                {
                    Game = Game,
                    Library = Library,
                    Saves = Saves?.ToList()
                });

                // Вызываем обновление интерфейса
                GameLoaded?.Invoke();
            }
        }

        public async Task GetGameAsync(bool ignoreCache)
        {
            if (!ignoreCache)
            {
                if(!InLibrary)
                {
                    // Проверяем, есть ли данные в кэше
                    var cachedGameResponse = GameData.LoadGameData(GameId);
                    if (cachedGameResponse != null)
                    {
                        // Данные загружены из кэша
                        Game = cachedGameResponse.Game;
                        if (cachedGameResponse.Saves != null)
                        {
                            Saves = new ObservableCollection<Save>(cachedGameResponse.Saves);
                        }
                        FilePath = GameData.GetFilePath(Game)!;
                        ExeExists = !string.IsNullOrEmpty(FilePath);

                        // Обновляем библиотеку, если она есть в кэше
                        Library = cachedGameResponse.Library;
                        UpdateLibraryDetails(Library);
                        GameLoaded?.Invoke();
                        return;
                    }
                }
                
            }

            // Загружаем с сервера, если нет данных в кэше или нужно обновить
            (var res, var body) = await FetchAsync<GameResponse>(
                HttpMethod.Get, $"games/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (res.IsSuccessStatusCode)
            {
                Game = body!.Game;
                FilePath = GameData.GetFilePath(Game)!;
                ExeExists = !string.IsNullOrEmpty(FilePath);

                Library = body.Library;
                UpdateLibraryDetails(Library);

                if(body.Saves != null)
                {
                    Saves = new ObservableCollection<Save>(body.Saves);
                }
                // Сохраняем новые данные в кэш
                GameData.SaveGameData(body);
                Debug.WriteLine($"Данные для игры '{GameId}' сохранены в кэше.");
            }

            GameLoaded?.Invoke();
        }

        private void UpdateLibraryDetails(Library? library)
        {
            if (library != null)
            {
                LastPlayedText = library.LastPlayedAt.HasValue
                    ? $"Последний запуск {GetDaysAgoText(library.LastPlayedAt.Value)}"
                    : "Последний запуск: Никогда";
                PlayedHoursText = $"Сыграно {(library.TimePlayed ?? 0) / 3600} часов";
                IsFavorite = library.IsFavorite;
                InLibrary = true;
            }
            else
            {
                LastPlayedText = "Последний запуск: Никогда";
                PlayedHoursText = "Сыграно 0 часов";
                InLibrary = false;
            }
            OnPropertyChanged(nameof(LastPlayedText));
            OnPropertyChanged(nameof(PlayedHoursText));
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(InLibrary));
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
