using PSB.Api.Response;
using PSB.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static PSB.Utils.Fetch;


namespace PSB.Helpers
{
    public static class FindSaves
    {
        public static async Task<string> GetValidGamePath(IGame game)
        {
            try
            {
                // Получаем путь от сервера
                string rawPath = await GetPath(game);

                if (string.IsNullOrEmpty(rawPath))
                {
                    Debug.WriteLine("Путь не получен от сервера");
                    return string.Empty;
                }

                string expandedPath = Environment.ExpandEnvironmentVariables(rawPath);
                Debug.WriteLine($"Расширенный путь: {expandedPath}");

                // Проверяем существование папки
                if (Directory.Exists(expandedPath))
                {
                    Debug.WriteLine("Папка найдена");
                    return expandedPath;
                }
                else
                {
                    Debug.WriteLine("Папка не существует");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке пути: {ex.Message}");
                return string.Empty;
            }
        }

        // Ваш существующий метод (немного модифицированный)
        public static async Task<string> GetPath(IGame game)
        {
            (var res, var body) = await FetchAsync<PathResponse>(HttpMethod.Get, $"games/{game.Id}/path");
            if (res.IsSuccessStatusCode && body != null)
            {
                Debug.WriteLine("Полученный путь: " + body.Path);
                return body.Path;
            }
            return string.Empty;
        }
        /// Ищет папку Saves в указанной директории, на уровень выше и в подпапках.
        public static string? FindSavesFolder(string? startDirectory)
        {
            if (string.IsNullOrEmpty(startDirectory))
                return null;

            string? currentDirectory = startDirectory;

            for (int i = 0; i < 1; i++) // Проверяем текущий уровень и на уровень выше
            {
                string? savesFolder = FindSavesInDirectory(currentDirectory);
                if (savesFolder != null)
                    return savesFolder;

                // Поднимаемся на уровень выше
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (currentDirectory == null)
                    break;
            }

            return null;
        }

        /// Проверяет наличие папки Saves в указанной директории, включая вложенные папки.
        public static string? FindSavesInDirectory(string directory)
        {
            try
            {
                var directories = Directory.GetDirectories(directory);
                foreach (var dir in directories)
                {
                    if (string.Equals(Path.GetFileName(dir), "saves", StringComparison.OrdinalIgnoreCase))
                        return dir;

                    // Рекурсивный поиск внутри вложенных папок
                    string? found = FindSavesInDirectory(dir);
                    if (found != null)
                        return found;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine($"Нет доступа к: {directory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при поиске папки: {ex.Message}");
            }

            return null;
        }
    }
}
