using System;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class LibraryDataManager<T> where T : IGame
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Генерация уникального ключа для библиотеки
        private static string GetLibraryKey(T game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game), "Game cannot be null.");
            }
            return $"{game.Type}_{game.Id}_Library";
        }

        // Сохранить данные библиотеки
        public static void SaveLibrary(T game, Library library)
        {
            try
            {
                LocalSettings.Values[GetLibraryKey(game)] = JsonSerializer.Serialize(library);
                Debug.WriteLine($"Данные библиотеки для игры '{game.Id}' (тип: {game.Type}) успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных библиотеки: {ex.Message}");
            }
        }

        // Загрузить данные библиотеки
        public static Library? LoadLibrary(T game)
        {
            try
            {
                var key = GetLibraryKey(game);
                if (LocalSettings.Values.TryGetValue(key, out var libraryJson))
                {
                    return JsonSerializer.Deserialize<Library>(libraryJson.ToString()!);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных библиотеки: {ex.Message}");
            }

            return null;
        }

        // Удалить данные библиотеки
        public static void RemoveLibrary(T game)
        {
            try
            {
                var key = GetLibraryKey(game);
                LocalSettings.Values.Remove(key);
                Debug.WriteLine($"Данные библиотеки для игры '{game.Id}' (тип: {game.Type}) удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных библиотеки: {ex.Message}");
            }
        }
    }
}