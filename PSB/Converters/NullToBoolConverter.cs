using Microsoft.UI.Xaml.Data;
using System;

namespace PSB.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Если parameter == "True", инвертируем результат
            bool invert = parameter?.ToString() == "True";
            bool result = value != null;
            return invert ? !result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
