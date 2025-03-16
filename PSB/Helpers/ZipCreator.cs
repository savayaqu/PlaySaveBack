using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Input;

namespace PSB.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class ZipCreator
    {
        public async Task<(string folderName, string zipFilePath, string hash, ulong sizeInBytes)> CreateZip(string folderPath, string gameName, string saveVersion)
        {
            // Получаем путь к рабочему столу
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Получаем имя папки
            string folderName = new DirectoryInfo(folderPath).Name;

            // Создаём путь для ZIP-архива на рабочем столе
            string zipFilePath = Path.Combine($"{desktopPath}/PlaySaveBack/{gameName}/{saveVersion}", $"{folderName}.zip");

            // Создаём ZIP-архив
            await ZipFolder(folderPath, zipFilePath);

            // Вычисляем хеш архива
            string hash = CalculateFileHash(zipFilePath);
            ulong sizeInBytes = (ulong)new FileInfo(zipFilePath).Length;
            // Возвращаем кортеж с именем папки, путём к ZIP-файлу и хешем
            return (folderName, zipFilePath, hash, sizeInBytes);
        }

        private async Task<string> ZipFolder(string folderPath, string zipFilePath)
        {
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
                    using (var entryStream = entry.Open())
                    using (var fileStream = File.OpenRead(file))
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            // Устанавливаем фиксированную дату и время для ZIP-архива
            File.SetCreationTime(zipFilePath, new DateTime(2001, 1, 1, 0, 0, 0));
            File.SetLastWriteTime(zipFilePath, new DateTime(2001, 1, 1, 0, 0, 0));

            return zipFilePath;
        }

        private string GetRelativePath(string fullPath, string basePath)
        {
            return Path.GetRelativePath(basePath, fullPath);
        }

        private string CalculateFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
