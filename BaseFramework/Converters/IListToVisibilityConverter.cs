using BaseFramework.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace BaseFramework.Converters {
	public class IListToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is null) {
				return Visibility.Collapsed;
			} else if (value is IList list) {
				return (list.Count != 0).ToVisibility();
			} else if (value is int count) {
				return (count != 0).ToVisibility();
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
