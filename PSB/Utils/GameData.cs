using System;
using System.Diagnostics;
using PSB.Api.Response;
using System.Text.Json;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class GameData
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Сохранить всю информацию об игре
        public static void SaveGameData(GameResponse gameResponse)
        {
            try
            {
                var game = gameResponse.Game;
                var library = gameResponse.Library;

                LocalSettings.Values[$"{game.Id}_Game"] = JsonSerializer.Serialize(game);
                if (library != null)
                {
                    LocalSettings.Values[$"{game.Id}_Library"] = JsonSerializer.Serialize(library);
                }
                Debug.WriteLine($"Данные для игры '{game.Id}' успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных игры: {ex.Message}");
            }
        }

        // Загрузить всю информацию об игре
        public static GameResponse? LoadGameData(ulong gameId)
        {
            try
            {
                if (LocalSettings.Values.TryGetValue($"{gameId}_Game", out var gameJson) &&
                    LocalSettings.Values.TryGetValue($"{gameId}_Library", out var libraryJson))
                {
                    var game = JsonSerializer.Deserialize<Game>(gameJson.ToString());
                    var library = JsonSerializer.Deserialize<Library>(libraryJson.ToString());

                    return new GameResponse { Game = game, Library = library };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных игры: {ex.Message}");
            }

            return null;
        }

        // Удалить данные игры
        public static void RemoveGameData(Game game)
        {
            try
            {
                LocalSettings.Values.Remove($"{game.Id}_Game");
                LocalSettings.Values.Remove($"{game.Id}_Library");
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