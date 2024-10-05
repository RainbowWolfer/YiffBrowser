using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using YB.E621.Models.E621;

namespace YB.E621.Converters {
	public class E621TagCategoryBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is E621TagCategory category) {
				Color color = E621Tag.GetCategoryColor(category);
				return new SolidColorBrush(color);
			}
			return Brushes.Black;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
