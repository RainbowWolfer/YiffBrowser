using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace YiffBrowser.BaseFramework.Converters;

public class NumberToKBConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        long number;
        if (value is long l) {
            number = l;
        } else {
            number = NumberHelper.ConvertInt(value);
        }
        if (parameter != null) {
            return number.FileSizeToKB(/*true*/);
        } else {
            return number.FileSizeToKB();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
