using System.Globalization;
using System.Windows.Data;
using System.Windows.Media; 

namespace HMI.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOnline && isOnline)
            return new SolidColorBrush(Colors.LimeGreen);

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}