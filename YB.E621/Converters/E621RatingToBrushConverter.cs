using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using YB.E621.Helpers;
using YB.E621.Models.E621;

namespace YB.E621.Converters {
	public class E621RatingToBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is E621Rating rating) {
				return new SolidColorBrush(InternalHelper.GetRatingColor(rating));
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
