using BaseFramework.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace YB.E621.Converters {
	public class TagsDisplayConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string[] array) {
				if (array.IsEmpty()) {
					return "ArrayEmpty";
				} else if (array.Length == 1 && array[0].IsBlank()) {
					return "Default";
				} else {
					return string.Join(" ", array);
				}
			}
			return "null";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
