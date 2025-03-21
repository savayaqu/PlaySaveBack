using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils.Game
{
    public class BaseDataManager<TData>
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        private static readonly JsonSerializerOptions JsonOptions = new() { TypeInfoResolver = GameJsonContext.Default };

        protected static string GetKey(string type, ulong gameId, string suffix) => $"{type}_{gameId}_{suffix}";

        public static void SaveData(string type, ulong gameId, TData data, string suffix)
        {
            try
            {
                LocalSettings.Values[GetKey(type, gameId, suffix)] = JsonSerializer.Serialize(data, JsonOptions);
                Debug.WriteLine($"Данные {suffix} для игры '{gameId}' (тип: {type}) успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении данных {suffix}: {ex.Message}");
            }
        }

        public static TData? LoadData(string type, ulong gameId, string suffix)
        {
            try
            {
                var key = GetKey(type, gameId, suffix);
                if (LocalSettings.Values.TryGetValue(key, out var json) && json is string jsonString)
                {
                    return JsonSerializer.Deserialize<TData>(jsonString, JsonOptions);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных {suffix}: {ex.Message}");
            }

            return default;
        }

        public static void RemoveData(string type, ulong gameId, string suffix)
        {
            try
            {
                var key = GetKey(type, gameId, suffix);
                LocalSettings.Values.Remove(key);
                Debug.WriteLine($"Данные {suffix} для игры '{gameId}' (тип: {type}) удалены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении данных {suffix}: {ex.Message}");
            }
        }
    }

}
