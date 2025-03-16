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
        private static string GetGameKey(T game) => $"{game.Type}_{game.Id}_Game";
        private static string GetGameKey(ulong gameId, string type) => $"{type}_{gameId}_Game";

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
        public static T? LoadGame(ulong gameId, string type)
        {
            try
            {
                var key = GetGameKey(gameId, type);
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
        public static void RemoveGame(ulong gameId, string type)
        {
            try
            {
                var key = GetGameKey(gameId, type);
                LocalSettings.Values.Remove(key);
                Debug.WriteLine($"Данные игры '{gameId}' (тип: {type}) удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных игры: {ex.Message}");
            }
        }
    }
}