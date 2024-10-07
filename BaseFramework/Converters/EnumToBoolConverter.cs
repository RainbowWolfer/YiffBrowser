using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class EnumToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is Enum e && parameter is Enum p) {
				if (e.Equals(p)) {
					return true;
				} else {
					return false;
				}
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
