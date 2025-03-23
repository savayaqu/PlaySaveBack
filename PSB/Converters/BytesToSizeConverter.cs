using System;
using Microsoft.UI.Xaml.Data;

namespace PSB.Converters
{
    public class BytesToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ulong bytes)
            {
                // Конвертируем байты в КБ, МБ или ГБ
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                double size = bytes;

                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }

                // Округляем до двух знаков после запятой
                return $"{size:0.##} {sizes[order]}";
            }

            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
