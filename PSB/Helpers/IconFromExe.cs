using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PSB.Helpers
{
    public static class IconFromExe
    {
        public static Icon? GetExeIcon(string exePath)
        {
            try
            {
                return Icon.ExtractAssociatedIcon(exePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при извлечении иконки: {ex.Message}");
                return null;
            }
        }
        public static IconElement? GetIconElement(string exePath)
        {
            try
            {
                // Извлекаем иконку из EXE-файла
                var icon = Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return null;

                // Преобразуем иконку в BitmapImage
                using (var bitmap = icon.ToBitmap())
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(memoryStream.AsRandomAccessStream());

                    // Создаем ImageIcon и устанавливаем BitmapImage как источник
                    return new ImageIcon { Source = bitmapImage };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при извлечении иконки: {ex.Message}");
                return null;
            }
        }
    }
}
