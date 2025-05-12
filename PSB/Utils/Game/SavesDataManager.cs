using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils.Game
{
    public static class SavesDataManager<T> where T : IGame
    {
        public static void SaveSaves(T game, List<Save> saves) => BaseDataManager<List<Save>>.SaveData(game.Type, game.Id, saves, "Saves");
        public static List<Save>? LoadSaves(string type, ulong gameId) => BaseDataManager<List<Save>>.LoadData(type, gameId, "Saves");
        public static void RemoveSaves(string type, ulong gameId) => BaseDataManager<List<Save>>.RemoveData(type, gameId, "Saves");

        // Удаление из локального хранилища сохранений, принадлежавшим конкретному UserCloudService
        public static void RemoveAllSavesByCloudServiceId(ulong? cloudServiceId)
        {
            var prefix = $"{AuthData.User.Id}_";
            var savesSuffix = "_Saves";

            var keysToProcess = ApplicationData.Current.LocalSettings.Values
                .Where(kvp => kvp.Key.StartsWith(prefix) && kvp.Key.EndsWith(savesSuffix))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToProcess)
            {
                if (ApplicationData.Current.LocalSettings.Values[key] is string json)
                {
                    try
                    {
                        var saves = JsonSerializer.Deserialize<List<Save>>(json, new JsonSerializerOptions { TypeInfoResolver = GameJsonContext.Default });
                        if (saves == null) continue;

                        var filtered = saves.Where(s => !(s.UserCloudServiceId == cloudServiceId && s.IsSynced)).ToList();
                        if (filtered.Count != saves.Count)
                        {
                            ApplicationData.Current.LocalSettings.Values[key] = JsonSerializer.Serialize(filtered, new JsonSerializerOptions { TypeInfoResolver = GameJsonContext.Default });
                            Debug.WriteLine($"Обновлены сохранения для ключа {key}: удалены {saves.Count - filtered.Count} сохранений.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при обработке ключа {key}: {ex.Message}");
                    }
                }
            }
        }

    }
}