using BaseFramework.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class StringToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string str) {
				return str.IsNotBlank().ToVisibility();
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
