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
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance?.ProfileViewModel!;
        public static GameViewModel? Instance { get; private set; }
        [ObservableProperty] public partial ulong GameId {  get; set; }
        [ObservableProperty] public partial string Type{  get; set; }
        [ObservableProperty] public partial IGame Game { get; set; }
        [ObservableProperty] public partial Library Library { get; set; }
        [ObservableProperty] public partial string LastPlayedText {  get; set; }
        [ObservableProperty] public partial string PlayedHoursText { get; set; }
        [ObservableProperty] public partial Boolean IsFavorite { get; set; }
        [ObservableProperty] public partial Boolean InLibrary { get; set; } = false;
        [ObservableProperty] public partial string FilePath { get; set; }
        [ObservableProperty] public partial string FolderPath { get; set; }
        [ObservableProperty] public partial InfoBar SuccessInfoBar { get; set; }
        [ObservableProperty] public partial Boolean IsUploading { get; set; }
        [ObservableProperty] public partial string SaveDescription { get; set; } = "";
        [ObservableProperty] public partial string SaveVersion { get; set; } = "";
        public SaveManager SaveManager => App.SaveManager;

        public event Action? GameLoaded;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LaunchGameCommand))]
        public partial Boolean ExeExists { get; set; } = false;

        public string FavoriteIcon => IsFavorite ? "\uEB52" : "\uEB51";

        partial void OnIsFavoriteChanged(Boolean value)
        {
            OnPropertyChanged(nameof(FavoriteIcon));
        }
        public GameViewModel(ulong gameId, string type)
        {

            Instance = this;
            GameId = gameId;
            Type = type.ToLower();
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
        [RelayCommand]
        private async Task CreateSave()
        {
            if (FolderPath == null) return;

            var zipCreator = new ZipCreator();
            var (folderName, zipPath, hash, size) = await zipCreator.CreateZip(FolderPath, Game.Name, SaveVersion);

            var newSave = new Save
            {
                FileId = folderName,
                FileName = $"{folderName}.zip",
                Version = SaveVersion,
                LastSyncAt = null,
                GameId = GameId,
                Description = SaveDescription,
                Hash = hash,
                Size = size,
                IsSynced = false,
                ZipPath = zipPath,
                CreatedAt = DateTime.Now,
            };

            SaveManager.AddSave(newSave);
            SaveManager.Test += 1;
            Debug.WriteLine("количество несинхр" + SaveManager.UnsyncedSavesCount);
            SavesDataManager<IGame>.SaveSaves(Game, SaveManager.Saves.ToList());
        }
        [RelayCommand]
        private async Task SyncSave(Save save)
        {
            if(save.IsSynced == true) return;
            SaveVersion = save.Version;
            SaveDescription = save.Description;
            bool uploadSuccess = await UploadFile(save.ZipPath);
            if (uploadSuccess)
            {
                SaveVersion = "";
                SaveDescription = "";

                SaveManager.MarkAsSynced(save);
                GameLoaded?.Invoke();
            }
        }
        [RelayCommand]
        private async Task OverwriteSave()
        {
            Debug.WriteLine("кнопка перезаписи");
        }
        [RelayCommand]
        public async Task RestoreSave()
        {
            Debug.WriteLine("Кнопка нажата");

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
                        HttpMethod.Patch, $"library/{Type}/{GameId}/update",
                        isFetch => Debug.WriteLine("isFetch " + isFetch),
                        error => Debug.WriteLine("error " + error),
                        new UpdateLibraryGameRequest(Library.TimePlayed, endTime.ToString("yyyy-MM-dd HH:mm:ss")),
                        serialize: true
                    );

                    // Сохраняем обновленные данные с использованием новых менеджеров
                    GameDataManager.SaveGame(Game);
                    LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                    if (SaveManager.Saves != null)
                    {
                        SavesDataManager<IGame>.SaveSaves(Game, SaveManager.Saves.ToList());
                    }

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
                HttpMethod.Patch, $"library/{Type}/{GameId}",
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
        public async Task<bool> UploadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("Файл не найден.");
                return false;
            }

            try
            {
                IsUploading = true;

                // Читаем файл в массив байтов
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                // Создаем MultipartFormDataContent
                var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(fileBytes), "file", Path.GetFileName(filePath) },
            { new StringContent(SaveVersion), "version" },
            { new StringContent(GameId.ToString()), "game_id" },
            { new StringContent(SaveDescription), "description" }
        };

                // Создаем HttpClient
                using var httpClient = new HttpClient();

                // Устанавливаем базовый URL
                httpClient.BaseAddress = new Uri("https://savayaqu.duckdns.org/playsaveback/api/");

                // Добавляем Bearer Token в заголовки
                if (!string.IsNullOrEmpty(AuthData.Token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthData.Token);
                }

                // Отправляем запрос
                var response = await httpClient.PostAsync("google-drive/upload", content);

                // Логируем статус ответа
                Debug.WriteLine($"Статус ответа: {response.StatusCode}");

                // Логируем заголовки ответа
                Debug.WriteLine("Заголовки ответа:");
                foreach (var header in response.Headers)
                {
                    Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                // Читаем ответ как строку
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Ответ от сервера (сырой): " + responseContent);

                // Проверяем, что ответ не пустой
                if (string.IsNullOrEmpty(responseContent))
                {
                    Debug.WriteLine("Ответ от сервера пустой.");
                    return false;
                }

                // Если статус ответа 401 Unauthorized
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("Ошибка авторизации: Неверный или отсутствующий токен.");
                    return false;
                }

                // Если статус ответа не успешный
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Ошибка при загрузке файла: {response.StatusCode}");
                    return false;
                }

                // Десериализуем ответ в объект Save
                try
                {
                    var updatedSave = JsonSerializer.Deserialize<Save>(responseContent);
                    if (updatedSave == null)
                    {
                        Debug.WriteLine("Не удалось десериализовать ответ от сервера.");
                        return false;
                    }

                    // Находим текущее сохранение в коллекции Saves
                    var existingSave = SaveManager.Saves.FirstOrDefault(s => s.FileId == updatedSave.FileId);
                    if (existingSave != null)
                    {
                        // Обновляем данные текущего сохранения
                        existingSave.FileName = updatedSave.FileName;
                        existingSave.Version = updatedSave.Version;
                        existingSave.Size = updatedSave.Size;
                        existingSave.Description = updatedSave.Description;
                        existingSave.LastSyncAt = updatedSave.LastSyncAt;
                        existingSave.Hash = updatedSave.Hash;
                        existingSave.IsSynced = true;

                        // Уведомляем интерфейс об изменениях
                        OnPropertyChanged(nameof(SaveManager.Saves));
                        GameLoaded?.Invoke();
                    }

                    Debug.WriteLine("Сохранение успешно синхронизировано и обновлено.");
                    return true;
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"Ошибка десериализации JSON: {jsonEx.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
            finally
            {
                IsUploading = false;
            }
        }

        [RelayCommand]
        public async Task UploadSave()
        {
            string zipFilePath = string.Empty;
            try
            {
                // Шаг 2: Отправляем ZIP-файл на сервер
                bool uploadSuccess = await UploadFile(zipFilePath);

                if (uploadSuccess)
                {
                    _ = GetGameAsync(true);
                    OnPropertyChanged(nameof(SaveManager.Saves)); // Дополнительно уведомляем об изменении

                    // Показываем уведомление об успехе
                    SuccessInfoBar.Title = "Успешно";
                    SuccessInfoBar.Message = "Сохранения успешно загружены на сервер.";
                    SuccessInfoBar.Severity = InfoBarSeverity.Success;
                    SuccessInfoBar.IsOpen = true;
                    SaveDescription = "";
                    SaveVersion = "";
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
        }
        [RelayCommand]
        public async Task DeleteSave(Save save)
        {
            if(save.IsSynced == false)
            {
                SaveManager.Saves?.Remove(save);
                return;
            }
            try
            {
                // Отправляем запрос на удаление файла
                var res = await FetchAsync(
                    HttpMethod.Delete,
                    $"google-drive/delete/{save.FileId}",
                    setError: e => Debug.WriteLine($"Error: {e}")
                );

                if (res.IsSuccessStatusCode)
                {
                    // Удаляем файл из локального списка
                    SaveManager.Saves?.Remove(save);

                    // Показываем уведомление об успехе
                    SuccessInfoBar.Title = "Успешно";
                    SuccessInfoBar.Message = "Сохранение успешно удалено.";
                    SuccessInfoBar.Severity = InfoBarSeverity.Success;
                    SuccessInfoBar.IsOpen = true;

                    Debug.WriteLine($"Сохранение {save.FileName} удалено.");
                }
                else
                {
                    // Показываем уведомление об ошибке
                    SuccessInfoBar.Title = "Ошибка";
                    SuccessInfoBar.Message = "Не удалось удалить сохранение.";
                    SuccessInfoBar.Severity = InfoBarSeverity.Error;
                    SuccessInfoBar.IsOpen = true;

                    Debug.WriteLine($"Ошибка при удалении сохранения: {res.StatusCode}");
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
        }
        [RelayCommand]
        public async Task AddToLibrary()
        {
            (var res, var body) = await FetchAsync<Library>(
                HttpMethod.Post, $"library/{Type}/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if (!res.IsSuccessStatusCode)
                return;

            if (body != null)
            {
                Library = body;
                InLibrary = true;
                ProfileViewModel.Libraries.Add(Library);

                // Обновляем кэш с использованием новых менеджеров
                GameDataManager.SaveGame(Game);
                LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                if (SaveManager.Saves != null)
                {
                    SavesDataManager<IGame>.SaveSaves(Game, SaveManager.Saves.ToList());
                }

                // Вызываем обновление интерфейса
                GameLoaded?.Invoke();
            }
        }
        [RelayCommand]
        public async Task GetMySaves()
        {
            (var res, var body) = await FetchAsync<MySavesGameResponse>(
                HttpMethod.Get, $"saves/{Type}/{GameId}/my",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (!res.IsSuccessStatusCode || body == null)
                return;

            // Логируем JSON-ответ
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Debug.WriteLine(bodyJson);

            // Сохраняем локальные несинхронизированные сохранения
            var localSaves = SaveManager.Saves?.Where(s => !s.IsSynced).ToList() ?? new List<Save>();

            // Очистка коллекции
            SaveManager.Saves.Clear();

            // Добавление сохранений с сервера
            foreach (var item in body.Save)
            {
                item.IsSynced = true;
                SaveManager.Saves.Add(item);
            }

            // Добавляем обратно локальные сохранения
            foreach (var localSave in localSaves)
            {
                SaveManager.Saves.Add(localSave);
            }

            // Сохраняем сохранения с использованием нового менеджера
            SavesDataManager<IGame>.SaveSaves(Game, [.. SaveManager.Saves]);
        }

        public async Task GetGameAsync(bool ignoreCache)
        {            
            if (!ignoreCache)
            {
                if (!InLibrary)
                {
                    // Проверяем, есть ли данные в кэше
                    var cachedGame = GameDataManager.LoadGame(Type, GameId);
                    var cachedLibrary = LibraryDataManager<IGame>.LoadLibrary(Type, GameId);
                    var cachedSaves = SavesDataManager<IGame>.LoadSaves(Type, GameId);

                    if (cachedGame != null)
                    {
                        // Данные загружены из кэша
                        Game = cachedGame;
                        if (cachedSaves != null)
                        {
                            SaveManager.Saves = new ObservableCollection<Save>(cachedSaves);
                        }
                        FilePath = PathDataManager<IGame>.GetFilePath(Game)!;
                        FolderPath = PathDataManager<IGame>.GetSavesFolderPath(Game)!;
                        ExeExists = !string.IsNullOrEmpty(FilePath);

                        // Обновляем библиотеку, если она есть в кэше
                        Library = cachedLibrary;
                        UpdateLibraryDetails(Library);
                        GameLoaded?.Invoke();
                        return;
                    }
                }
            }

            // Загружаем с сервера, если нет данных в кэше или нужно обновить
            (var res, var body) = await FetchAsync<GameResponse>(
                HttpMethod.Get, $"{Type}s/{GameId}",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (res.IsSuccessStatusCode)
            {
                Game = body!.Game;
                FilePath = PathDataManager<IGame>.GetFilePath(Game)!;
                FolderPath = PathDataManager<IGame>.GetSavesFolderPath(Game)!;
                ExeExists = !string.IsNullOrEmpty(FilePath);

                Library = body.Library;
                UpdateLibraryDetails(Library);

                if (body.Saves != null)
                {
                    SaveManager.Saves = new ObservableCollection<Save>(body.Saves);
                }

                // Сохраняем новые данные в кэш с использованием новых менеджеров
                GameDataManager.SaveGame(Game);
                LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                SavesDataManager<IGame>.SaveSaves(Game, body.Saves?.ToList() ?? new List<Save>());
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
