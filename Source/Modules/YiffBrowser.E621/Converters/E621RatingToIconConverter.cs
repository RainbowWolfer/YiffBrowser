using System.Globalization;
using System.Windows.Data;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Converters;

public class E621RatingToIconConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is E621Rating rating) {
            return InternalHelper.GetRatingIcon(rating);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
