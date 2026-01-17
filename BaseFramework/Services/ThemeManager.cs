using BaseFramework.Controls;
using HandyControl.Themes;
using RW.Base.WPF.DependencyInjections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Threading;

namespace BaseFramework.Services;

public interface IThemeManager : ISingletonDependency {
	void ToggleTheme();
}

internal class ThemeManager() : IThemeManager {

	private class _ControlzEx {
		public static readonly Assembly AssemblyControlzEx = Assembly.Load("ControlzEx");
		public static readonly Type DwmHelper = AssemblyControlzEx.GetType("ControlzEx.Internal.DwmHelper", throwOnError: true)!;
		public static readonly MethodInfo SetImmersiveDarkMode = DwmHelper.GetMethod("SetImmersiveDarkMode", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
	}


	[SecurityCritical]
	public void ToggleTheme() {
		bool isDark;
		HandyControl.Themes.ThemeManager c = HandyControl.Themes.ThemeManager.Current;
		if (c.ActualApplicationTheme is ApplicationTheme.Dark) {
			c.ApplicationTheme = ApplicationTheme.Light;
			isDark = false;
		} else {
			c.ApplicationTheme = ApplicationTheme.Dark;
			isDark = true;
		}

		if (Application.Current != null) {
			IEnumerable<WindowBase> windows = Application.Current.Windows.OfType<WindowBase>();
			foreach (WindowBase window in windows) {
				nint handle = window.WindowHandle;
				if (handle != IntPtr.Zero) {
					window.Dispatcher.BeginInvoke(new Action(() => {
						object? r = _ControlzEx.SetImmersiveDarkMode.Invoke(null, [handle, isDark]);
						Debug.WriteLine(r);

						//window.InvalidateArrange();
						//window.InvalidateMeasure();
						//window.InvalidateVisual();
						//window.UpdateDefaultStyle();
						//window.UpdateLayout();

						if (window.WindowState == WindowState.Maximized) {
							window.WindowState = WindowState.Normal;
							double width = window.Width;
							window.Width = width + 1;
							window.Width = width;
							window.WindowState = WindowState.Maximized;
						} else {
							double width = window.Width;
							window.Width = width + 1;
							window.Width = width;
						}

						//SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

						//WindowBackdropType type = WindowBackdropManager.GetBackdropType(window);
						//WindowBackdropManager.UpdateBackdrop(window, type);
						// 在 ToggleTheme 循环中：
						//_ = method.Invoke(null, [handle, isDark]);
						//window.Dispatcher.BeginInvoke(new Action(() => {
						//	// 传入一个极其微小的边距（全 0 或其中一个为 1）
						//	//MARGINS margins = new() { cyTopHeight = 1 };
						//	//DwmExtendFrameIntoClientArea(handle, ref margins);

						//	// 如果是最大化状态，再调用一次刷新
						//	SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

						//	// 在循环中：
						//	//int darkMode = isDark ? 1 : 0;
						//	//// 尝试两个版本的属性 ID
						//	//if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)) != 0) {
						//	//	DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
						//	//}

						//}), System.Windows.Threading.DispatcherPriority.Input);

					}), DispatcherPriority.Render);
				}
			}
		}

	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOZORDER = 0x0004;
	private const uint SWP_FRAMECHANGED = 0x0020; // 关键：触发框架改变消息

	[StructLayout(LayoutKind.Sequential)]
	public struct MARGINS {
		public int cxLeftWidth;
		public int cxRightWidth;
		public int cyTopHeight;
		public int cyBottomHeight;
	}

	[DllImport("dwmapi.dll")]
	public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);


	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
}
