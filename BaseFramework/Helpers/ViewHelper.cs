using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace BaseFramework.Helpers {
	public static class ViewHelper {

		public static bool SafeClose(this Window? window) {
			if (window is null) {
				return false;
			}
			try {
				window.Close();
				return true;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return false;
			}
		}

		public static Color HexToColor(this string hex) {
			// 移除前导的 '#' 符号
			hex = hex.Replace("#", string.Empty);

			// 将十六进制字符串转换为整数
			byte a = 255; // 默认不透明
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			// 创建颜色对象
			return Color.FromArgb(a, r, g, b);
		}

		public static Visibility ToVisibility(this bool b, bool reverse = false) {
			if (reverse) {
				return b ? Visibility.Collapsed : Visibility.Visible;
			} else {
				return b ? Visibility.Visible : Visibility.Collapsed;
			}
		}

	}
}
