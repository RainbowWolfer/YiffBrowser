using System.Globalization;
using System.Windows.Data;

namespace YiffBrowser.BaseFramework.Converters;

public class PlaybackTickToTimeSpanConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is long l) {
			TimeSpan timeSpan = TimeSpan.FromTicks(l);
			return $"{timeSpan:mm\\:ss}";
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
