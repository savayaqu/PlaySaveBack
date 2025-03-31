using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.ContentDialogs;
using PSB.Interfaces;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using PSB.Utils.Game;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    //TODO: добавитьь к каждой команде проверку на то что её можно нажать + есть ли папка с сохранениями и т.д.
    public partial class GameViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance?.ProfileViewModel!;
        public static GameViewModel? Instance { get; private set; }
        [ObservableProperty] public partial ulong GameId { get; set; }
        [ObservableProperty] public partial string Type { get; set; }
        [ObservableProperty] public partial IGame Game { get; set; }
        [ObservableProperty] public partial Library Library { get; set; }
        [ObservableProperty] public partial string LastPlayedText { get; set; }
        [ObservableProperty] public partial string PlayedHoursText { get; set; }
        [ObservableProperty] public partial Boolean IsFavorite { get; set; } = false;
        [ObservableProperty] public partial Boolean InLibrary { get; set; }
        [ObservableProperty] public partial string FilePath { get; set; }
        [ObservableProperty] public partial string FolderPath { get; set; }
        [ObservableProperty] public partial Boolean IsUploading { get; set; }
        [ObservableProperty] public partial string SaveDescription { get; set; } = "";
        [ObservableProperty] public partial string SaveVersion { get; set; } = "";
        [ObservableProperty] public partial ObservableCollection<Save> Saves { get; set; } = new ObservableCollection<Save>();
        public event Action? GameLoaded;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LaunchGameCommand))]
        public partial Boolean ExeExists { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateSaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(OverwriteSaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(RestoreSaveCommand))]
        public partial Boolean FolderSavesExists { get; set; } = false;

        partial void OnFolderPathChanged(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                FolderSavesExists = false;
            }
            else
            {
                FolderSavesExists = true;
            }
            OnPropertyChanged(nameof(FolderSavesExists));
        }

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

            _ = GetGameAsync(false).ContinueWith(_ =>
            {
                GameLoaded?.Invoke();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }
        [RelayCommand(CanExecute = nameof(FolderSavesExists))]
        private async Task CreateSave()
        {
            bool versionExist = Saves?.Any(s => s.Version == SaveVersion) ?? false;
            //TODO: доработать, запретить нажимать кнопку, если версия сходится
            if (versionExist)
            {
                SaveVersion = "go to na xyu";
                return;
            }
            if (FolderPath == null)
                return;
            Debug.WriteLine("Folder Path " + FolderPath);

            var (folderName, zipPath, hash, size) = await App.ZipHelper!.CreateZip(FolderPath, Game.Name, SaveVersion);

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
            Saves.Add(newSave);
            SavesDataManager<IGame>.SaveSaves(Game, Saves.ToList());
        }
        [RelayCommand]
        private async Task SyncSave(Save save)
        {
            if (save.IsSynced == true) return;
            SaveVersion = save.Version;
            SaveDescription = save.Description;
            bool uploadSuccess = await UploadFile(save);
            if (uploadSuccess)
            {
                SaveVersion = "";
                SaveDescription = "";
                // Если стоит галочка в настройках об удалении с пк после синхронизации - удаление с пк
                if (SettingsData.DeleteLocalSaveAfterSync == true)
                {
                    App.ZipHelper!.DeleteFile(save.ZipPath);
                }
                Saves = new ObservableCollection<Save>(Saves);
                SavesDataManager<IGame>.SaveSaves(Game, Saves.ToList());
                OnPropertyChanged(nameof(Saves));
                NotificationService.ShowSuccess("Сохранение успешно синхронизировано");
            }
        }
        [RelayCommand(CanExecute = nameof(FolderSavesExists))]
        private async Task OverwriteSave()
        {
            Debug.WriteLine("кнопка перезаписи");
        }
        [RelayCommand(CanExecute = nameof(FolderSavesExists))]
        public async Task RestoreSave(Save save)
        {
            if(save.IsSynced == false)
            {
                string folderPath = PathDataManager<IGame>.GetSavesFolderPath(Game);
                await App.ZipHelper!.RestoreFromZip(save.ZipPath, folderPath);
                Debug.WriteLine("Сохранения восстановлены");
                NotificationService.ShowSuccess("Сохранения восстановлены");
                return;
            }
            string zipFilePath = Path.Combine(Path.GetTempPath(), "game_saves_restore", "saves_backup.zip");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));

                // 1. Загрузка с повторными попытками
                Debug.WriteLine("Начинаем загрузку архива...");
                Debug.WriteLine("FileId сохранения " + save.FileId);
                if(save.FileId == null)
                {
                    Debug.WriteLine("save.FileId is null" + save.FileId);
                    return;
                }

                var res = await FetchAsync(HttpMethod.Get, $"google-drive/download/{save.FileId}");
                if (res != null && res.IsSuccessStatusCode)
                {
                    // Асинхронно сохраняем содержимое
                    await using (var fileStream = File.Create(zipFilePath))
                    {
                        await res.Content.CopyToAsync(fileStream);
                    }
                }

                // 2. Проверка архива
                if (!await App.ZipHelper!.ZipFileValid(zipFilePath))
                {
                    Debug.WriteLine("Архив поврежден после загрузки");
                    NotificationService.ShowError("Архив поврежден после загрузки");
                    return;
                }

                // 3. Восстановление
                string folderPath = PathDataManager<IGame>.GetSavesFolderPath(Game);
                await App.ZipHelper!.RestoreFromZip(zipFilePath, folderPath);

                NotificationService.ShowSuccess("Восстановление завершено успешно");
                Debug.WriteLine("Восстановление завершено успешно");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                NotificationService.ShowError($"Ошибка: {ex.Message}");

                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);
                throw;
            }
            finally
            {
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);
            }
        }
        [RelayCommand(CanExecute = nameof(ExeExists))]
        public async Task LaunchGame()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.WriteLine($"Файл не найден: {FilePath}");
                    return;
                }

                using (Process gameProcess = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = FilePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(FilePath), // Рабочая папка
                        CreateNoWindow = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false
                    };

                    gameProcess.StartInfo = startInfo;
                    gameProcess.EnableRaisingEvents = true;

                    var tcs = new TaskCompletionSource<bool>();
                    gameProcess.Exited += (sender, e) => tcs.TrySetResult(true);
                    DateTime startTime = DateTime.Now;

                    try
                    {
                        gameProcess.Start();
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        Debug.WriteLine("Ошибка запуска: " + ex.Message);
                        NotificationService.ShowError("Ошибка запуска: " + ex.Message);
                        return;
                    }

                    Debug.WriteLine($"Игра запущена: {FilePath}");
                    await tcs.Task;

                    DateTime endTime = DateTime.Now;
                    TimeSpan playTime = endTime - startTime;
                    uint secondsPlayed = (uint)playTime.TotalSeconds;



                    Library.TimePlayed = (Library.TimePlayed ?? 0) + secondsPlayed;
                    Library.LastPlayedAt = endTime;
                    OnPropertyChanged(nameof(Library));


                    try
                    {
                        var res = await FetchAsync(
                            HttpMethod.Patch,
                            $"library/{Type}/{GameId}/update",
                            new UpdateLibraryGameRequest(Library.TimePlayed, endTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            serialize: true
                        );

                        GameDataManager.SaveGame(Game);
                        LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                        if (Saves != null)
                        {
                            SavesDataManager<IGame>.SaveSaves(Game, Saves.ToList());
                        }
                        UpdateLibraryDetails(Library);
                    }
                    catch (HttpRequestException ex)
                    {
                        Debug.WriteLine("Ошибка соединения: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
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
            var dialog = new GameSettingsContentDialog(Game, this);
            await App.DialogService.ShowDialogAsync(dialog);
        }


        [RelayCommand]
        public async Task ToggleFavorite()
        {
            var res = await FetchAsync(HttpMethod.Patch, $"library/{Type}/{GameId}");

            if (!res.IsSuccessStatusCode)
                return;

            IsFavorite = !IsFavorite;
            // Обновляем библиотеку
            var libraryItem = ProfileViewModel.Libraries.FirstOrDefault(l => l.Game?.Id == GameId);
            if (libraryItem != null)
            {
                libraryItem.IsFavorite = IsFavorite;
                Library.IsFavorite = IsFavorite;
                LibraryDataManager<IGame>.SaveLibrary(Game, Library);
            }
            App.LibraryService!.UpdateLibraryMenu();
        }
        public async Task<bool> UploadFile(Save save)
        {
            if (!File.Exists(save.ZipPath))
            {
                Debug.WriteLine("Файл не найден.");
                return false;
            }

            try
            {
                IsUploading = true;

                // Читаем файл в массив байтов
                var fileBytes = await File.ReadAllBytesAsync(save.ZipPath);

                // Создаем MultipartFormDataContent
                var content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(fileBytes), "file", Path.GetFileName(save.ZipPath) },
                    { new StringContent(SaveVersion), "version" },
                    { new StringContent(GameId.ToString()), Game is SideGame ? "side_game_id" : "game_id" },
                    { new StringContent(SaveDescription), "description" }
                };

               (var res, var body) = await FetchAsync<Save>(HttpMethod.Post, "google-drive/upload", body: content);


                // Если статус ответа не успешный
                if (!res.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Ошибка при загрузке файла: {res.StatusCode}");
                    return false;
                }
                if(body != null)
                {
                    var index = Saves.IndexOf(save);
                    if (index != -1)
                    {
                        Debug.WriteLine("сохранение найдено");
                        body.IsSynced = true;
                        Saves[index] = body;
                        SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
                    }

                }
                return true;

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
        public async Task DeleteSave(Save save)
        {
            if (save.IsSynced == false)
            {
                // Удаление из коллекции
                Saves?.Remove(save);
                // Удаление с пк
                App.ZipHelper!.DeleteFile(save.ZipPath);
                SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
                NotificationService.ShowSuccess($"Сохранение {save.FileName} {save.Version} удалено.");
                return;
            }
            try
            {
                // Отправляем запрос на удаление файла
                var res = await FetchAsync(HttpMethod.Delete,$"google-drive/delete/{save.FileId}");

                if (res.IsSuccessStatusCode)
                {
                    // Удаляем файл из локального списка и с пк
                    Saves?.Remove(save);
                    if (save.ZipPath != null)
                    {
                        App.ZipHelper!.DeleteFile(save.ZipPath);
                    }
                    Debug.WriteLine($"Сохранение {save.FileName} удалено.");
                    NotificationService.ShowSuccess($"Сохранение {save.FileName} {save.Version} удалено.");

                }
                else
                {
                    Debug.WriteLine($"Ошибка при удалении сохранения: {res.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                NotificationService.ShowError($"Ошибка: {ex.Message}");
            }
        }
        [RelayCommand]
        public async Task AddToLibrary()
        {
            (var res, var body) = await FetchAsync<Library>(HttpMethod.Post, $"library/{Type}/{GameId}");
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
                if (Saves != null)
                {
                    SavesDataManager<IGame>.SaveSaves(Game, Saves.ToList());
                }

                // Вызываем обновление интерфейса
                GameLoaded?.Invoke();
                NotificationService.ShowSuccess($"Игра {Game.Name} добавлена в библиотеку");

            }
        }
        [RelayCommand]
        public async Task GetMySaves()
        {
            (var res, var body) = await FetchAsync<MySavesGameResponse>(HttpMethod.Get, $"saves/{Type}/{GameId}/my");

            if (!res.IsSuccessStatusCode || body == null)
                return;

            // Логируем JSON-ответ
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Debug.WriteLine(bodyJson);

            // Сохраняем локальные несинхронизированные сохранения
            var localSaves = Saves?.Where(s => !s.IsSynced).ToList() ?? new List<Save>();

            // Очистка коллекции
            Saves.Clear();

            // Добавление сохранений с сервера
            foreach (var item in body.Save)
            {
                item.IsSynced = true;
                Saves.Add(item);
            }

            // Добавляем обратно локальные сохранения
            foreach (var localSave in localSaves)
            {
                Saves.Add(localSave);
            }

            // Сохраняем сохранения с использованием нового менеджера
            SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
        }

        public async Task GetGameAsync(bool ignoreCache)
        {
            try
            {
                if (!ignoreCache && !InLibrary)
                {
                    // Загрузка из кэша
                    var cachedGame = GameDataManager.LoadGame(Type, GameId);
                    var cachedLibrary = LibraryDataManager<IGame>.LoadLibrary(Type, GameId);
                    var cachedSaves = SavesDataManager<IGame>.LoadSaves(Type, GameId);

                    if (cachedGame != null)
                    {
                        Game = cachedGame;
                        Saves = new ObservableCollection<Save>(cachedSaves ?? new List<Save>());
                        Library = cachedLibrary;

                        FilePath = PathDataManager<IGame>.GetFilePath(Game) ?? string.Empty;
                        FolderPath = PathDataManager<IGame>.GetSavesFolderPath(Game) ?? string.Empty;
                        ExeExists = !string.IsNullOrEmpty(FilePath);

                        UpdateLibraryDetails(Library);
                        GameLoaded?.Invoke();
                        Debug.WriteLine("Данные загружены из кэша");
                        return;
                    }
                }

                // Загрузка с сервера
                (var res, var body) = await FetchAsync<GameResponse>(HttpMethod.Get, $"{Type}s/{GameId}");

                if (!res.IsSuccessStatusCode) return;

                // Обработка Game/SideGame
                if (body.Game != null)
                {
                    Game = body.Game;
                }
                else if (body.SideGame != null)
                {
                    Game = body.SideGame;
                }
                else
                {
                    Debug.WriteLine("Ошибка: и Game, и SideGame равны null");
                    return;
                }

                // Обработка Library
                Library = body.Library;
                UpdateLibraryDetails(Library);

                // Получение путей
                FilePath = PathDataManager<IGame>.GetFilePath(Game) ?? string.Empty;
                FolderPath = PathDataManager<IGame>.GetSavesFolderPath(Game) ?? string.Empty;
                ExeExists = !string.IsNullOrEmpty(FilePath);


                // Сохранение в кэш
                if (Game != null)
                {
                    GameDataManager.SaveGame(Game);
                    if (Library != null)
                    {
                        LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                    }
                    // Обработка сохранений
                    if (body.Saves != null)
                    {
                        foreach (var save in body.Saves)
                        {
                            save.IsSynced = true;
                        }
                        Saves = new ObservableCollection<Save>(body.Saves ?? new List<Save>());
                        SavesDataManager<IGame>.SaveSaves(Game, Saves.ToList());
                    }
                    Debug.WriteLine($"Данные для игры '{GameId}' сохранены в кэше.");
                }

                GameLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критическая ошибка в GetGameAsync: {ex.Message}\n{ex.StackTrace}");
                // Дополнительная обработка ошибки
            }
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
