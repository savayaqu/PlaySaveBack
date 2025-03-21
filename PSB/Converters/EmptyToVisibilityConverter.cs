using System;
using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PSB.Converters
{
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Проверяем, является ли значение пустым
            bool isEmpty = IsEmpty(value);

            // Если параметр "True", инвертируем логику
            bool invert = parameter as string == "True";

            // Возвращаем Visible или Collapsed в зависимости от значения и инверсии
            return (invert ? isEmpty : !isEmpty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // Обратное преобразование не требуется
        }

        private bool IsEmpty(object value)
        {
            if (value == null)
                return true;

            if (value is string str)
                return string.IsNullOrEmpty(str);

            if (value is ICollection collection)
                return collection.Count == 0;

            if (value is IEnumerable enumerable)
                return !enumerable.GetEnumerator().MoveNext();

            return false; // Если значение не null и не пустое
        }
    }
}
