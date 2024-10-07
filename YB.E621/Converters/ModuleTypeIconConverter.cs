using BaseFramework.Enums;
using System.Globalization;
using System.Windows.Data;

namespace YB.E621.Converters {
	public class ModuleTypeIconConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is ModuleType type) {
				return type switch {
					ModuleType.E621 => @"/BaseFramework;component/Resources/Icons/E621.png",
					ModuleType.E6AI => @"/BaseFramework;component/Resources/Icons/E6AI2.png",
					ModuleType.E926 => @"/BaseFramework;component/Resources/Icons/E621.png",
					_ => throw new NotImplementedException(),
				};
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
