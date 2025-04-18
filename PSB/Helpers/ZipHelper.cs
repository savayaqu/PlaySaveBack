﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using PSB.Utils;

namespace PSB.Helpers
{
    public class ZipHelper
    {
        public static async Task<(string folderName, string zipFilePath, string hash, ulong sizeInBytes)> CreateZip(string folderPath, string gameName, string saveVersion)
        {
            // Получаем путь к месту сохранения
            string UserPath = SettingsData.PathToLocalSaves;

            // Получаем имя папки
            string folderName = new DirectoryInfo(folderPath).Name;

            // Создаём путь для ZIP-архива 
            string zipFilePath = Path.Combine($"{UserPath}/PlaySaveBack/Saves/{gameName}/{saveVersion}", $"{folderName}.zip");

            // Создаём ZIP-архив
            await ZipFolder(folderPath, zipFilePath);

            // Вычисляем хеш архива
            string hash = CalculateFileHash(zipFilePath);
            ulong sizeInBytes = (ulong)new FileInfo(zipFilePath).Length;
            // Возвращаем кортеж с именем папки, путём к ZIP-файлу и хешем
            return (folderName, zipFilePath, hash, sizeInBytes);
        }

        private static async Task<string> ZipFolder(string folderPath, string zipFilePath)
        {
            // Проверяем, существует ли исходная папка
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(
                    $"Исходная папка не найдена: {folderPath}. " +
                    "Проверьте путь и повторите попытку.");
            }

            // Проверяем, существует ли папка для ZIP-архива
            string zipDirectory = Path.GetDirectoryName(zipFilePath);
            if (!Directory.Exists(zipDirectory))
            {
                Directory.CreateDirectory(zipDirectory); // Создаём папку, если её нет
            }

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath); // Удаляем существующий ZIP-файл, если он есть
            }

            // Получаем список файлов в папке и сортируем их по имени
            var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                                 .OrderBy(f => f)
                                 .ToList();

            // Создаём ZIP-архив
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var entry = zipArchive.CreateEntry(GetRelativePath(file, folderPath), CompressionLevel.Optimal);

                    // Устанавливаем исходную дату изменения для файла внутри архива
                    entry.LastWriteTime = new FileInfo(file).LastWriteTime;

                    // Копируем содержимое файла в архив
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(file);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            // Устанавливаем фиксированную дату и время для ZIP-архива
            File.SetCreationTime(zipFilePath, new DateTime(2001, 1, 1, 0, 0, 0));
            File.SetLastWriteTime(zipFilePath, new DateTime(2001, 1, 1, 0, 0, 0));

            return zipFilePath;
        }

        public static string GetRelativePath(string fullPath, string basePath)
        {
            return Path.GetRelativePath(basePath, fullPath);
        }

        public static string CalculateFileHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexStringLower(hashBytes);
        }
        public static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден.");
                return;
            }
            try
            {
                // Получаем информацию о родительской папке
                string parentDirectory = Path.GetDirectoryName(filePath)!;
                // Удаляем файл
                File.Delete(filePath);
                Console.WriteLine($"Файл {filePath} успешно удалён.");

                // Проверяем, пуста ли папка после удаления
                if (Directory.Exists(parentDirectory) &&
                    !Directory.EnumerateFileSystemEntries(parentDirectory).Any())
                {
                    // Удаляем пустую папку
                    Directory.Delete(parentDirectory);
                    Console.WriteLine($"Папка {parentDirectory} удалена, так как она пуста.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
        }
        public static async Task<string> CreateBackup(string folderPath, string gameName, string saveVersion)
        {

            // Папка temp
            string zipFilePath = Path.Combine(Path.GetTempPath(), $"PlaySaveBack/Backups/{gameName}/{saveVersion}");

            // Создаём ZIP-архив
            await ZipFolder(folderPath, zipFilePath);

            return zipFilePath;
        }
        // В классе ZipCreator добавляем новый метод
        public static void RestoreFromZip(string zipFilePath, string targetFolderPath)
        {
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("ZIP файл не найден", zipFilePath);
            }

            try
            {
                // 1. Создаем временную папку для безопасного восстановления
                string tempRestorePath = Path.Combine(Path.GetTempPath(), "temp_restore_" + Guid.NewGuid());
                Directory.CreateDirectory(tempRestorePath);

                // 2. Распаковываем во временную папку
                ZipFile.ExtractToDirectory(zipFilePath, tempRestorePath);

                // 3. Очищаем целевую папку (если существует)
                if (Directory.Exists(targetFolderPath))
                {
                    Directory.Delete(targetFolderPath, true);
                }
                Directory.CreateDirectory(targetFolderPath);

                // 4. Копируем файлы из временной папки в целевую
                foreach (var dirPath in Directory.GetDirectories(tempRestorePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(tempRestorePath, targetFolderPath));
                }

                foreach (var filePath in Directory.GetFiles(tempRestorePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(filePath, filePath.Replace(tempRestorePath, targetFolderPath), true);
                }

                // 5. Очищаем временные файлы
                Directory.Delete(tempRestorePath, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при восстановлении из ZIP: {ex.Message}");
                throw; // Перебрасываем исключение для обработки выше
            }
        }
        // Метод для проверки целостности ZIP-архива
        public static bool ZipFileValid(string filePath)
        {
            try
            {
                using var archive = ZipFile.OpenRead(filePath);
                var entries = archive.Entries;
                return entries.Count > 0; // Простая проверка
            }
            catch
            {
                return false;
            }
        }
    }
}
