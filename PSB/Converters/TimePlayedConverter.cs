using System;
using Microsoft.UI.Xaml.Data;

namespace PSB.Converters
{
    public class TimePlayedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint seconds)
            {
                if (seconds <= 0)
                {
                    return "0 секунд";
                }
                if (seconds < 60)
                {
                    return $"{seconds} секунд";
                }
                else if (seconds < 3600)
                {
                    uint minutes = seconds / 60;
                    return $"{minutes} минут";
                }
                else
                {
                    uint hours = seconds / 3600;
                    uint remainingMinutes = (seconds % 3600) / 60;
                    return $"{hours} часов {remainingMinutes} минут";
                }
            }
            return "0 минут";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
