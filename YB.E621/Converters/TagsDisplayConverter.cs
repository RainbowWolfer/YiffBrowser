using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace YB.E621.Converters;

public class TagsDisplayConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string[] array) {
            if (array.IsEmpty()) {
				//return "ArrayEmpty";
				return string.Empty;
			} else if (array.Length == 1 && array[0].IsBlank()) {
				//return "Default";
				return string.Empty;
            } else {
                return string.Join(" ", array);
            }
        }
        //return "null";
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
