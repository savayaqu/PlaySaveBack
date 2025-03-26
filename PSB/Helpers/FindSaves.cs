using System;
using System.Diagnostics;
using System.IO;

namespace PSB.Helpers
{
    public static class FindSaves
    {
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
