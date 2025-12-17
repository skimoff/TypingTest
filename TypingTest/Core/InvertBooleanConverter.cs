using System.Globalization;
using System.Windows.Data;

namespace TypingTest.Core;

public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        // Повертаємо оригінальне значення, якщо це не булевий тип
        return value;
    }

    // Зворотне перетворення (не використовується)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        return value;
    }
}