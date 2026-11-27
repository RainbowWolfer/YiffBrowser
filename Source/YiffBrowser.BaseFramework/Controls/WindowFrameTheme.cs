using System.Runtime.InteropServices;
using System.Windows.Interop;
using HandyControl.Themes;

namespace YiffBrowser.BaseFramework.Controls;

internal static class WindowFrameTheme {
	public static bool IsDark() {
		return ThemeManager.Current.ActualApplicationTheme is ApplicationTheme.Dark;
	}

	public static void Apply(nint handle, bool isDark) {
		if (handle == 0) {
			return;
		}

		int value = isDark ? 1 : 0;
		if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int)) != 0) {
			DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
		}
	}

	public static void Apply(System.Windows.Window window, bool isDark) {
		nint handle = new WindowInteropHelper(window).Handle;
		Apply(handle, isDark);
	}

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);
}
