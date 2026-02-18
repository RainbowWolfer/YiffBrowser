using HandyControl.Themes;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Events;
using RW.Base.WPF.Interfaces;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Controls;
using YiffBrowser.BaseFramework.Events;

namespace YiffBrowser.BaseFramework.Services;

public interface IThemeManager : ISingletonDependency {
	event TypedEventHandler<IThemeManager, ThemeChangedEventArgs>? ThemeChanged;

	bool IsDarkTheme();
	void ToggleTheme();
}

internal class ThemeManager(IEventAggregator eventAggregator) : IThemeManager, IAppInitialize {
	public event TypedEventHandler<IThemeManager, ThemeChangedEventArgs>? ThemeChanged;

	private class _ControlzEx {
		public static readonly Assembly AssemblyControlzEx = Assembly.Load("ControlzEx");
		public static readonly Type DwmHelper = AssemblyControlzEx.GetType("ControlzEx.Internal.DwmHelper", throwOnError: true)!;
		public static readonly MethodInfo SetImmersiveDarkMode = DwmHelper.GetMethod("SetImmersiveDarkMode", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
	}

	string IAppInitialize.Description => "";
	int IPriority.Priority => IntPriority.Higher;

	void IAppInitialize.AppInitialize(IStatusReport statusReport) {
		SetCustomResources(IsDarkTheme());
	}

	public void SetCustomResources(bool isDark) {
		const string ThemePrefix = "/YiffBrowser.Resources;component/";
		// 1. 确定目标文件的完整 URI 路径
		string themeName = isDark ? "Dark.xaml" : "Light.xaml";
		Uri newThemeUri = new($"{ThemePrefix}{themeName}", UriKind.RelativeOrAbsolute);

		// 2. 创建新的资源字典
		ResourceDictionary newDict = new() { Source = newThemeUri };

		// 3. 获取当前全局资源集合
		Collection<ResourceDictionary> mergedDicts = Application.Current.Resources.MergedDictionaries;

		// 4. 核心逻辑：替换旧的主题资源
		// 遍历已有的字典，寻找并替换包含主题文件名的资源
		bool themeReplaced = false;

		for (int i = 0; i < mergedDicts.Count; i++) {
			ResourceDictionary dict = mergedDicts[i];
			if (dict.Source != null && dict.Source.OriginalString.Contains(ThemePrefix) && (
				dict.Source.OriginalString.Contains("Light.xaml")
				|| dict.Source.OriginalString.Contains("Dark.xaml")
			)) {
				mergedDicts[i] = newDict; // 直接替换索引位置，效率最高且防止闪烁
				themeReplaced = true;
				break;
			}
		}

		// 5. 如果初始化时没找到旧主题（保险措施），则直接添加
		if (!themeReplaced) {
			mergedDicts.Add(newDict);
		}
	}

	public bool IsDarkTheme() {
		HandyControl.Themes.ThemeManager c = HandyControl.Themes.ThemeManager.Current;
		return c.ActualApplicationTheme is ApplicationTheme.Dark;
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
						//Debug.WriteLine(r);

						window.InvalidateArrange();
						window.InvalidateMeasure();
						window.InvalidateVisual();
						window.UpdateDefaultStyle();
						window.UpdateLayout();

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

						SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

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

		SetCustomResources(isDark);

		ThemeChangedEventArgs args = new(isDark);
		eventAggregator.GetEvent<ThemeChangedEvent>().Publish(args);
		ThemeChanged?.Invoke(this, args);

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
