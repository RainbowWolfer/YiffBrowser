using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class TimeDurationConverter : IValueConverter {
		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null) {
				return value;
			}

			string? raw = value.ToString();
			return Convert(raw);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}

		public static string Convert(string? str) {
			if (double.TryParse(str, out double seconds)) {
				TimeSpan result = TimeSpan.FromSeconds(seconds);
				if (result.Minutes < 1) {
					string minSec = $"{result.Seconds}s";
					return minSec;
				} else {
					string minSec = $"{result.Minutes}m:{result.Seconds}s";
					return minSec;
				}
			}
			return str ?? string.Empty;
		}
	}
}
