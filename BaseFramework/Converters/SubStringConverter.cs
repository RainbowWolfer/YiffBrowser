using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class SubStringConverter : IValueConverter {
		public int StartIndex { get; set; } = 0;
		public int Length { get; set; } = 1;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is null) {
				return string.Empty;
			}
			return $"{value}".Substring(StartIndex, Length);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
