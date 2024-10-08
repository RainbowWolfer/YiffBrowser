﻿using System.Globalization;
using System.Windows.Data;

namespace BaseFramework.Converters {
	public class BoolToBoolReConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool boolValue) {
				return !boolValue;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool boolValue) {
				return !boolValue;
			}
			return value;
		}
	}
}
