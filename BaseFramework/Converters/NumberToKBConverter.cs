using BaseFramework.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class NumberToKBConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			long number;
			if (value is long l) {
				number = l;
			} else {
				number = Math.Abs(System.Convert.ToInt64(value));
			}
			if (parameter != null) {
				return number.FileSizeToKB(true);
			} else {
				return number.FileSizeToKB();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
