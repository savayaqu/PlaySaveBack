using System;
using Windows.Storage;

namespace PSB.Utils
{
    public static class SettingsData
    {
        private static readonly ApplicationDataContainer LocalSettings =
            ApplicationData.Current.LocalSettings;

        private static bool _deleteLocalSaveAfterSync;
        private static string _pathToLocalSaves;

        static SettingsData()
        {
            // Инициализация значений при первом обращении
            _deleteLocalSaveAfterSync = LoadSetting(nameof(DeleteLocalSaveAfterSync), false);
            _pathToLocalSaves = LoadSetting(nameof(PathToLocalSaves),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        public static bool DeleteLocalSaveAfterSync
        {
            get => _deleteLocalSaveAfterSync;
            set
            {
                if (_deleteLocalSaveAfterSync != value)
                {
                    _deleteLocalSaveAfterSync = value;
                    SaveSetting(nameof(DeleteLocalSaveAfterSync), value);
                }
            }
        }

        public static string PathToLocalSaves
        {
            get => _pathToLocalSaves;
            set
            {
                if (_pathToLocalSaves != value)
                {
                    _pathToLocalSaves = value;
                    SaveSetting(nameof(PathToLocalSaves), value);
                }
            }
        }

        private static T LoadSetting<T>(string key, T defaultValue)
        {
            return LocalSettings.Values.TryGetValue(key, out object value)
                ? (T)value
                : defaultValue;
        }

        private static void SaveSetting<T>(string key, T value)
        {
            if (value == null)
            {
                LocalSettings.Values.Remove(key);
            }
            else
            {
                LocalSettings.Values[key] = value;
            }
        }
    }
}