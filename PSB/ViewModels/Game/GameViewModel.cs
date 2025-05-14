using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Api.Response.GoogleDrive.Save;
using PSB.ContentDialogs;
using PSB.Interfaces;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using PSB.Utils.Game;
using static System.Net.Mime.MediaTypeNames;
using Windows.ApplicationModel.DataTransfer;
using static PSB.Utils.Fetch;
using System.Text.Encodings.Web;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        public ProfileViewModel ProfileViewModel { get; set; } = MainWindow.Instance?.ProfileViewModel!;
        public static GameViewModel? Instance { get; private set; }
        [ObservableProperty] public partial ulong GameId { get; set; }
        [ObservableProperty] public partial string Type { get; set; }
        [ObservableProperty] public partial IGame Game { get; set; }
        [ObservableProperty] public partial Library Library { get; set; }
        [ObservableProperty] public partial Boolean IsFavorite { get; set; } = false;
        [ObservableProperty] public partial Boolean InLibrary { get; set; }
        [ObservableProperty] public partial string FilePath { get; set; }
        [ObservableProperty] public partial string FolderPath { get; set; }
        [ObservableProperty] public partial Boolean IsUploading { get; set; }

        [ObservableProperty] public partial CloudService? SelectedCloudService { get; set; } = null;
        [ObservableProperty] public partial string SaveDescription { get; set; } = string.Empty;
        [ObservableProperty] public partial string SaveVersion { get; set; } = string.Empty;
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

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OverwriteSaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateSaveCommand))]
        public partial Boolean VersionExists { get; set; } = false;
        public bool CanLaunchGame() => ExeExists && !App.GameLaunchService!.IsGameLaunched;

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
        partial void OnSaveVersionChanged(string value)
        {
            VersionExists = Saves?.Any(s => s.Version == value) ?? false;
            SaveVersion = value;
            OverwriteSaveCommand.NotifyCanExecuteChanged();
            CreateSaveCommand.NotifyCanExecuteChanged();
        }
        private bool CanCreateOverwriteSave()
        {
            return FolderSavesExists && !string.IsNullOrEmpty(SaveVersion) && !VersionExists;
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
        [RelayCommand(CanExecute = nameof(CanCreateOverwriteSave))]
        private async Task CreateSave()
        {
            if (FolderPath == null)
                return;
            Debug.WriteLine("Folder Path " + FolderPath);
            try
            {
                IsUploading = true;
                var (folderName, zipPath, hash, size) = await Helpers.ZipHelper.CreateZip(FolderPath, Game.Name, SaveVersion);

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
                    ZipPath = zipPath,
                    CreatedAt = DateTime.Now,
                };
                Saves.Add(newSave);
                SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsUploading = false;
            }
        }
        [RelayCommand(CanExecute = nameof(CanCreateOverwriteSave))]
        private async Task OverwriteSave(Save existingSave)
        {
            if (!existingSave.IsSynced)
            {
                try
                {
                    IsUploading = true;
                    // Создаем бэкап перед перезаписью
                    string backupPath = await Helpers.ZipHelper.CreateBackup(
                        FolderPath,
                        Game.Name,
                        $"{existingSave.Version}_backup_{DateTime.Now:yyyyMMdd_HHmmss}");

                    Debug.WriteLine($"Создан бэкап: {backupPath}");

                    // Перезаписываем сохранение
                    var (folderName, zipPath, hash, size) = await Helpers.ZipHelper.CreateZip(
                        FolderPath,
                        Game.Name,
                        existingSave.Version);

                    existingSave.FileName = $"{folderName}.zip";
                    existingSave.Hash = hash;
                    existingSave.Size = size;
                    existingSave.ZipPath = zipPath;
                    existingSave.Version = SaveVersion;
                    existingSave.Description = SaveDescription;
                    existingSave.CreatedAt = DateTime.Now;

                    // Удаляем старый файл, если путь изменился
                    if (existingSave.ZipPath != zipPath && File.Exists(existingSave.ZipPath))
                    {
                        Helpers.ZipHelper.DeleteFile(existingSave.ZipPath);
                    }

                    UpdateExistingSave(existingSave, existingSave);
                    NotificationService.ShowSuccess("Сохранение успешно перезаписано");
                }
                catch (Exception ex)
                {
                    NotificationService.ShowError($"Ошибка перезаписи: {ex.Message}");
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    SaveVersion = "";
                    SaveDescription = "";
                    IsUploading = false;
                }
            }
            if (existingSave.IsSynced)
            {
                try
                {

                    var connectedService = AuthData.ConnectedCloudServices.FirstOrDefault(s => s.UserCloudServiceId == existingSave.UserCloudServiceId);
                    if (connectedService != null)
                    {
                        SelectedCloudService = connectedService;
                        IsUploading = true;
                        // Создаем бэкап перед перезаписью
                        string backupPath = await Helpers.ZipHelper.CreateBackup(
                            FolderPath,
                            Game.Name,
                            $"{existingSave.Version}_backup_{DateTime.Now:yyyyMMdd_HHmmss}");

                        Debug.WriteLine($"Создан бэкап: {backupPath}");

                        // Перезаписываем сохранение
                        var (folderName, zipPath, hash, size) = await Helpers.ZipHelper.CreateZip(
                            FolderPath,
                            Game.Name,
                            existingSave.Version);

                        (bool success, Save updatedSave) = await CloudFileUploader.OverwriteFileAsync(existingSave, zipPath, SaveVersion, SaveDescription);

                        if (success && updatedSave != null)
                        {
                            SaveVersion = "";
                            SaveDescription = "";

                            UpdateExistingSave(existingSave, updatedSave);

                            NotificationService.ShowSuccess("Файл успешно перезаписан");
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.ShowError($"Ошибка перезаписи: {ex.Message}");
                }
                finally
                {
                    IsUploading = false;
                }
            }
        }
        [RelayCommand]
        private async Task SyncSave(Save save)
        {
            if (save.IsSynced == true || SelectedCloudService == null)
                return;

            try
            {
                SaveVersion = save.Version;
                SaveDescription = save.Description;
                IsUploading = true;
                (bool uploadSuccess, Save? updatedSave ) = await CloudFileUploader.UploadFileAsync(
                    save,
                    SelectedCloudService,
                    Game,
                    SaveVersion,
                    SaveDescription);

                if (uploadSuccess)
                {
                    SaveVersion = "";
                    SaveDescription = "";

                    if (SettingsData.DeleteLocalSaveAfterSync)
                    {
                        Helpers.ZipHelper.DeleteFile(save.ZipPath);
                    }
                    if (updatedSave != null)
                    {
                        UpdateExistingSave(save, updatedSave);
                    }
                    NotificationService.ShowSuccess($"Сохранение синхронизировано с {SelectedCloudService.Name}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Ошибка синхронизации: {ex.Message}");
            }
            finally
            {
                IsUploading = false;
            }
        }
        private void UpdateExistingSave(Save oldSave, Save newSave)
        {
            var index = Saves.IndexOf(oldSave);
            // Обновляем в коллекции
            Saves[index] = newSave;
            SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
            OnPropertyChanged(nameof(Saves));
        }
        public void PrepareOverwrite(Save save)
        {
            SaveDescription = save.Description;
            SaveVersion = save.Version;
        }
        [RelayCommand(CanExecute = nameof(FolderSavesExists))]
        public async Task RestoreSave(Save save)
        {
            if(save.IsSynced == false)
            {
                string folderPath = PathDataManager<IGame>.GetSavesFolderPath(Game);
                Helpers.ZipHelper.RestoreFromZip(save.ZipPath, folderPath);
                Debug.WriteLine("Сохранения восстановлены");
                NotificationService.ShowSuccess("Сохранения восстановлены");
                return;
            }
            string zipFilePath = Path.Combine(Path.GetTempPath(), "game_saves_restore", "saves_backup.zip");
            Debug.WriteLine($"{zipFilePath}");

            try
            {
                IsUploading = true;
                Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));

                // 1. Загрузка с повторными попытками
                Debug.WriteLine("Начинаем загрузку архива...");
                Debug.WriteLine("FileId сохранения " + save.FileId);
                if(save.FileId == null)
                {
                    Debug.WriteLine("save.FileId is null" + save.FileId);
                    return;
                }

                var res = await FetchAsync(HttpMethod.Get, $"saves/{save.Id}/google-drive/download");
                if (res != null && res.IsSuccessStatusCode)
                {
                    // Асинхронно сохраняем содержимое
                    await using var fileStream = File.Create(zipFilePath);
                    await res.Content.CopyToAsync(fileStream);
                }

                // 2. Проверка архива
                if (!Helpers.ZipHelper.ZipFileValid(zipFilePath))
                {
                    Debug.WriteLine("Архив поврежден после загрузки");
                    NotificationService.ShowError("Архив поврежден после загрузки");
                    return;
                }

                // 3. Восстановление
                string folderPath = PathDataManager<IGame>.GetSavesFolderPath(Game);
                Helpers.ZipHelper.RestoreFromZip(zipFilePath, folderPath);

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
                IsUploading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanLaunchGame))]
        public async Task LaunchGame()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.WriteLine($"Файл не найден: {FilePath}");
                    return;
                }

                var result = await App.GameLaunchService!.LaunchGameAsync(FilePath);

                if (!result.Launched)
                    return;

                TimeSpan playTime = result.EndTime - result.StartTime;
                uint secondsPlayed = (uint)playTime.TotalSeconds;

                Library.TimePlayed = (Library.TimePlayed ?? 0) + secondsPlayed;
                Library.LastPlayedAt = result.EndTime;
                OnPropertyChanged(nameof(Library));

                try
                {
                    GameDataManager.SaveGame(Game);
                    LibraryDataManager<IGame>.SaveLibrary(Game, Library);
                    if (Saves != null)
                    {
                        SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
                    }
                    UpdateLibraryDetails(Library);
                    LastPlayedGameManager.SaveLastPlayedGame(Game, Library);

                    await FetchAsync(
                        HttpMethod.Patch,
                        $"library/{Type}/{GameId}/update",
                        new UpdateLibraryGameRequest(Library.TimePlayed, result.EndTime.ToString("yyyy-MM-dd HH:mm:ss")),
                        serialize: true
                    );
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine("Ошибка соединения: " + ex.Message);
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
            await App.DialogService!.ShowDialogAsync(new GameSettingsContentDialog(Game, this));
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
  
        [RelayCommand]
        public async Task DeleteSave(Save save)
        {
            if (save.IsSynced == false)
            {
                // Удаление из коллекции
                Saves?.Remove(save);
                // Удаление с пк
                Helpers.ZipHelper.DeleteFile(save.ZipPath);
                SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
                NotificationService.ShowSuccess($"Сохранение {save.FileName} {save.Version} удалено.");
                return;
            }
            try
            {
                IsUploading = true;
                // Отправляем запрос на удаление файла
                var res = await FetchAsync(HttpMethod.Delete,$"saves/{save.Id}/google-drive/delete");

                if (res.IsSuccessStatusCode)
                {
                    // Удаляем файл из локального списка и с пк
                    Saves?.Remove(save);
                    if (save.ZipPath != null)
                    {
                        Helpers.ZipHelper.DeleteFile(save.ZipPath);
                    }
                    SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
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
            finally
            {
                IsUploading = false;
            }
        }
        [RelayCommand]
        public async Task ShareSave(Save save)
        {
            if (save.IsSynced == true)
            {
                try
                {
                    (var res, var body) = await FetchAsync<ShareSaveResponse>(HttpMethod.Get, $"saves/{save.Id}/google-drive/share");
                    if (res.IsSuccessStatusCode)
                    {
                        if(body != null)
                        {
                            var dataPackage = new DataPackage();
                            dataPackage.SetText(body.Url);
                            Clipboard.SetContent(dataPackage);
                            NotificationService.ShowSuccess($"Ссылка для {save.FileName} {save.Version} сохранена в буфер обмена.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.ShowError($"Ошибка: {ex.Message}");
                }
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
                    SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
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
          
            // Сохраняем локальные несинхронизированные сохранения
            var localSaves = Saves?.Where(s => !s.IsSynced).ToList() ?? [];

           
            // Очистка коллекции
            Saves.Clear();
            
            // Добавление сохранений с сервера
            foreach (var item in body.Save)
            {
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
                        Saves = [.. cachedSaves ?? []];
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
                        Saves = [.. body.Saves ?? []];
                        SavesDataManager<IGame>.SaveSaves(Game, [.. Saves]);
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
                IsFavorite = library.IsFavorite;
                InLibrary = true;
            }
            else
            {
                InLibrary = false;
            }
            OnPropertyChanged(nameof(Library));
        }
    }
}
