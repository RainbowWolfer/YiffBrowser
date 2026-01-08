using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BaseFramework.Helpers;

public static class ViewHelper {

    public static Color HexToColor(this string hex) {
        // 移除前导的 '#' 符号
        hex = hex.Replace("#", string.Empty);

        // 将十六进制字符串转换为整数
        byte a = 255; // 默认不透明
        byte r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        // 创建颜色对象
        return Color.FromArgb(a, r, g, b);
    }


	public static void ActivateWindow(this Window? window) {
		if (window is null) {
			return;
		}
		window.Show();
		window.Activate();
		window.Focus();
		window.Dispatcher.Invoke(() => {
			window.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}, DispatcherPriority.Loaded);
	}

}
