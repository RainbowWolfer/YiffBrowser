﻿using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class IListToBoolReConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is IList list) {
				return (list.Count == 0);
			} else if (value is int count) {
				return (count == 0);
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
