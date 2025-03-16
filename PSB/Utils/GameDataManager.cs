using System;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using Windows.Storage;

namespace PSB.Utils
{
    public static class GameDataManager<T> where T : IGame
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Генерация уникального ключа для игры
        private static string GetGameKey(T game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game), "Game cannot be null.");
            }
            return $"{game.Type}_{game.Id}_Game";
        }
        // Сохранить данные игры
        public static void SaveGame(T game)
        {
            try
            {
                LocalSettings.Values[GetGameKey(game)] = JsonSerializer.Serialize(game);
                Debug.WriteLine($"Данные игры '{game.Id}' (тип: {game.Type}) успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных игры: {ex.Message}");
            }
        }

        // Загрузить данные игры
        public static T? LoadGame(T game)
        {
            try
            {
                var key = GetGameKey(game);
                if (LocalSettings.Values.TryGetValue(key, out var gameJson))
                {
                    return JsonSerializer.Deserialize<T>(gameJson.ToString()!);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных игры: {ex.Message}");
            }

            return default;
        }

        // Удалить данные игры
        public static void RemoveGame(T game)
        {
            try
            {
                var key = GetGameKey(game);
                LocalSettings.Values.Remove(key);
                Debug.WriteLine($"Данные игры '{game.Id}' (тип: {game.Type}) удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных игры: {ex.Message}");
            }
        }
    }
}