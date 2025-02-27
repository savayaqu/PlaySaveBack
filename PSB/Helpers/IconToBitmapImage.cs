using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PSB.Helpers
{
    public static class IconToBitmapImage
    {
        public static async Task<BitmapImage?> IconToBitmapImageAsync(Icon icon)
        {
            try
            {
                using (var bitmap = icon.ToBitmap())
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;

                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при преобразовании иконки: {ex.Message}");
                return null;
            }
        }
    }
}
