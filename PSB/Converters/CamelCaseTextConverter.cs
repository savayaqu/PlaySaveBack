using System;
using Microsoft.UI.Xaml.Data;

namespace PSB.Converters
{
    public class CamelCaseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string text)
            {
                // Вставляем пробелы перед заглавными буквами (кроме первой)
                return System.Text.RegularExpressions.Regex.Replace(
                    text,
                    "(\\B[A-Z])",
                    " $1");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
