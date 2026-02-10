using System.Globalization;
using System.Windows.Data;
using YiffBrowser.Resources.Icons;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.Converters;

public class ModuleTypeIconConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is ModuleType type) {
            return type switch {
                ModuleType.E621 => IconResources.E621_PNG,
                ModuleType.E6AI => IconResources.E6AI2_PNG,
                ModuleType.E926 => IconResources.E621_PNG,
                _ => throw new NotImplementedException(),
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
