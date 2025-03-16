using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class SavesDataManager<T> where T : IGame
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Генерация уникального ключа для сохранений
        private static string GetSavesKey(T game) => $"{game.Type}_{game.Id}_Saves";
        private static string GetSavesKey(ulong gameId, string type) => $"{type}_{gameId}_Saves";

        // Сохранить данные сохранений
        public static void SaveSaves(T game, List<Save> saves)
        {
            try
            {
                LocalSettings.Values[GetSavesKey(game)] = JsonSerializer.Serialize(saves);
                Debug.WriteLine($"Данные сохранений для игры '{game.Id}' (тип: {game.Type}) успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных сохранений: {ex.Message}");
            }
        }

        // Загрузить данные сохранений
        public static List<Save>? LoadSaves(ulong gameId, string type)
        {
            try
            {
                var key = GetSavesKey(gameId, type);
                if (LocalSettings.Values.TryGetValue(key, out var savesJson))
                {
                    return JsonSerializer.Deserialize<List<Save>>(savesJson.ToString()!);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных сохранений: {ex.Message}");
            }

            return null;
        }

        // Удалить данные сохранений
        public static void RemoveSaves(ulong gameId, string type)
        {
            try
            {
                var key = GetSavesKey(gameId, type);
                LocalSettings.Values.Remove(key);
                Debug.WriteLine($"Данные сохранений для игры '{gameId}' (тип: {type}) удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных сохранений: {ex.Message}");
            }
        }
    }
}