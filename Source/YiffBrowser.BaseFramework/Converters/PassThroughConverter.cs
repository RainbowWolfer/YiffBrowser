using System.Globalization;
using System.Windows.Data;

namespace YiffBrowser.BaseFramework.Converters;

public class PassThroughConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		return value;
	}
}
