using System;
using System.Diagnostics;
using PSB.Interfaces;
using Windows.Storage;

namespace PSB.Utils
{
    public static class PathDataManager<T> where T : IGame
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Генерация уникального ключа для FilePath
        private static string GetFilePathKey(T game) => $"{game.Type}_{game.Id}_FilePath";
        private static string GetFilePathKey(ulong gameId, string type) => $"{type}_{gameId}_FilePath";

        // Генерация уникального ключа для SavesFolderPath
        private static string GetSavesFolderPathKey(T game) => $"{game.Type}_{game.Id}_SavesFolderPath";
        private static string GetSavesFolderPathKey(ulong gameId, string type) => $"{type}_{gameId}_SavesFolderPath";

        // Получение FilePath
        public static string? GetFilePath(T game)
        {
            try
            {
                var key = GetFilePathKey(game);
                if (LocalSettings.Values.TryGetValue(key, out var value))
                {
                    return value as string;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении FilePath: {ex.Message}");
            }

            return null;
        }

        // Установка FilePath
        public static void SetFilePath(T game, string path)
        {
            try
            {
                var key = GetFilePathKey(game);
                LocalSettings.Values[key] = path;
                Debug.WriteLine($"FilePath для игры '{game.Id}' (тип: {game.Type}) успешно сохранен.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при установке FilePath: {ex.Message}");
            }
        }

        // Получение SavesFolderPath
        public static string? GetSavesFolderPath(T game)
        {
            try
            {
                var key = GetSavesFolderPathKey(game);
                if (LocalSettings.Values.TryGetValue(key, out var value))
                {
                    return value as string;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении SavesFolderPath: {ex.Message}");
            }

            return null;
        }

        // Установка SavesFolderPath
        public static void SetSavesFolderPath(T game, string path)
        {
            try
            {
                var key = GetSavesFolderPathKey(game);
                LocalSettings.Values[key] = path;
                Debug.WriteLine($"SavesFolderPath для игры '{game.Id}' (тип: {game.Type}) успешно сохранен.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при установке SavesFolderPath: {ex.Message}");
            }
        }
    }
}