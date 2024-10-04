using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class IListToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (parameter?.ToString() == "only1") {
				if (value is IList list) {
					return list.Count == 1;
				} else if (value is int count) {
					return count == 1;
				}
			} else {
				if (value is IList list) {
					return list.Count != 0;
				} else if (value is int count) {
					return count != 0;
				}
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
