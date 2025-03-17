using System;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class GameDataManager
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            TypeInfoResolver = GameJsonContext.Default
        };

        private static string GetGameKey(IGame game) => $"{game.Type}_{game.Id}_Game";

        public static void SaveGame(IGame game)
        {
            try
            {
                LocalSettings.Values[GetGameKey(game)] = JsonSerializer.Serialize(game, JsonOptions);
                Debug.WriteLine($"Данные игры '{game.Id}' (тип: {game.Type}) успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных игры: {ex.Message}");
            }
        }

        public static IGame? LoadGame(string type, ulong gameId)
        {
            try
            {
                var key = $"{type}_{gameId}_Game";
                if (LocalSettings.Values.TryGetValue(key, out var gameJson))
                {
                    Debug.WriteLine($"Данные игры '{gameId}' (тип: {type}) успешно загружены из кэша.");
                    return JsonSerializer.Deserialize<IGame>(gameJson.ToString()!, JsonOptions);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных игры: {ex.Message}");
            }
            return null;
        }
    }

}