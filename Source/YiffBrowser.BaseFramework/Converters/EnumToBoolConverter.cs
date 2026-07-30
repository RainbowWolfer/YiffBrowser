using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace YiffBrowser.BaseFramework.Converters;

public class EnumToBoolConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value == null || parameter == null) {
			return false;
		}

		return value.ToString() == parameter.ToString();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is bool b && b) {
			if (parameter is Enum enumValue) {
				return enumValue;
			}
			return Enum.Parse(targetType, parameter.SafeToString());
		}
		return Binding.DoNothing;
	}
}