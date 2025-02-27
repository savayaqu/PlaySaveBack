using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PSB.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = parameter as string == "True"; // Проверяем параметр
            bool boolValue = value is bool b && b;

            return (invert ? !boolValue : boolValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility visibility && visibility == Visibility.Visible);
        }
    }
}
