using BaseFramework.Helpers;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseFramework.Converters;
public class IListToVisibilityConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is null) {
			return Visibility.Collapsed;
		} else if (value is IList list) {
			return (list.Count != 0).ToVisibility();
		} else if (value is int count) {
			return (count != 0).ToVisibility();
		}
		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
