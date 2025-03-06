using System;
using System.Diagnostics;
using PSB.Api.Response;
using System.Text.Json;
using PSB.Models;
using Windows.Storage;
using System.Collections.Generic;

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
                var saves = gameResponse.Saves;
                LocalSettings.Values[$"{game.Id}_Game"] = JsonSerializer.Serialize(game);
                if (library != null)
                {
                    LocalSettings.Values[$"{game.Id}_Library"] = JsonSerializer.Serialize(library);
                }
                if (saves != null)
                {
                    LocalSettings.Values[$"{game.Id}_Saves"] = JsonSerializer.Serialize(saves);
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
                // Загружаем данные игры
                if (LocalSettings.Values.TryGetValue($"{gameId}_Game", out var gameJson))
                {
                    var game = JsonSerializer.Deserialize<Game>(gameJson.ToString()!);

                    // Загружаем данные библиотеки (если есть)
                    Library? library = null;
                    if (LocalSettings.Values.TryGetValue($"{gameId}_Library", out var libraryJson))
                    {
                        library = JsonSerializer.Deserialize<Library>(libraryJson.ToString()!);
                    }

                    // Загружаем данные сохранений (если есть)
                    List<Save>? saves = null;
                    if (LocalSettings.Values.TryGetValue($"{gameId}_Saves", out var savesJson))
                    {
                        saves = JsonSerializer.Deserialize<List<Save>>(savesJson.ToString()!);
                    }

                    // Возвращаем объект GameResponse
                    return new GameResponse
                    {
                        Game = game!,
                        Library = library,
                        Saves = saves
                    };
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
                LocalSettings.Values.Remove($"{game.Id}_Saves");
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