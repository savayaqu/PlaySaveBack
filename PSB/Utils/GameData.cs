using System;
using System.Diagnostics;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class GameData
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Записать данные для игры
        public static void SaveGameData(Game game, string executablePath, string savesFolderPath)
        {
            try
            {
                LocalSettings.Values[$"{game.Id}_FilePath"] = executablePath;
                LocalSettings.Values[$"{game.Id}_SavesFolderPath"] = savesFolderPath;
                Debug.WriteLine($"Данные для игры '{game.Id}' успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных игры: {ex.Message}");
            }
        }

        // Удалить данные игры
        public static void RemoveGameData(Game game)
        {
            try
            {
                LocalSettings.Values.Remove($"{game.Id}_FilePath");
                LocalSettings.Values.Remove($"{game.Id}_SavesFolderPath");
                Debug.WriteLine($"Данные для игры '{game.Id}' удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных игры: {ex.Message}");
            }
        }

        // Получение и установка путей по Game.Id
        public static string? GetFilePath(Game game) =>
            LocalSettings.Values.TryGetValue($"{game.Id}_FilePath", out var value) ? value as string : null;

        public static void SetFilePath(Game game, string path) =>
            LocalSettings.Values[$"{game.Id}_FilePath"] = path;

        public static string? GetSavesFolderPath(Game game) =>
            LocalSettings.Values.TryGetValue($"{game.Id}_SavesFolderPath", out var value) ? value as string : null;

        public static void SetSavesFolderPath(Game game, string path) =>
            LocalSettings.Values[$"{game.Id}_SavesFolderPath"] = path;
    }
}
