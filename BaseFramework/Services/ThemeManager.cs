using BaseFramework.Controls;
using HandyControl.Themes;
using RW.Base.WPF.DependencyInjections;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace BaseFramework.Services;

public interface IThemeManager : ISingletonDependency {
	void ToggleTheme();
}

internal class ThemeManager() : IThemeManager {
	private static Assembly assembly = Assembly.Load("ControlzEx");
	private static Type type = assembly.GetType("ControlzEx.Internal.DwmHelper", throwOnError: true)!;
	private static MethodInfo method = type.GetMethod("SetImmersiveDarkMode", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;

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
			foreach (WindowBase item in windows) {
				nint handle = item.WindowHandle;
				_ = method.Invoke(null, [handle, isDark]);
			}
		}

	}
}
