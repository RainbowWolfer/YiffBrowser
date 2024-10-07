using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseFramework.Converters {

	public class EnumToVisibilityReConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is Enum e && parameter is Enum p) {
				if (e.Equals(p)) {
					return Visibility.Collapsed;
				} else {
					return Visibility.Visible;
				}
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
