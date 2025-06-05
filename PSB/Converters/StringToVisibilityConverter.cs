using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace PSB.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = !string.IsNullOrEmpty(value as string);

            // Если параметр "true" - инвертируем логику
            if (parameter is string strParam && bool.TryParse(strParam, out bool invert))
            {
                isVisible = invert ? !isVisible : isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
