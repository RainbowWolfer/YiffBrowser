using HandyControl.Themes;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Events;
using RW.Base.WPF.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
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

	string IAppInitialize.Description => "";
	int IPriority.Priority => IntPriority.Higher;

	void IAppInitialize.AppInitialize(IStatusReport statusReport) {
		SetCustomResources(IsDarkTheme());
	}

	public void SetCustomResources(bool isDark) {
		const string ThemePrefix = "/YiffBrowser.Resources;component/";
		string themeName = isDark ? "Dark.xaml" : "Light.xaml";
		Uri newThemeUri = new($"{ThemePrefix}{themeName}", UriKind.RelativeOrAbsolute);
		ResourceDictionary newDict = new() { Source = newThemeUri };
		Collection<ResourceDictionary> mergedDicts = Application.Current.Resources.MergedDictionaries;

		bool themeReplaced = false;
		for (int i = 0; i < mergedDicts.Count; i++) {
			ResourceDictionary dict = mergedDicts[i];
			if (dict.Source != null && dict.Source.OriginalString.Contains(ThemePrefix) && (
				dict.Source.OriginalString.Contains("Light.xaml")
				|| dict.Source.OriginalString.Contains("Dark.xaml")
			)) {
				mergedDicts[i] = newDict;
				themeReplaced = true;
				break;
			}
		}

		if (!themeReplaced) {
			mergedDicts.Add(newDict);
		}
	}

	public bool IsDarkTheme() {
		return WindowFrameTheme.IsDark();
	}

	public void ToggleTheme() {
		bool isDark;
		HandyControl.Themes.ThemeManager theme = HandyControl.Themes.ThemeManager.Current;
		if (theme.ActualApplicationTheme is ApplicationTheme.Dark) {
			theme.ApplicationTheme = ApplicationTheme.Light;
			isDark = false;
		} else {
			theme.ApplicationTheme = ApplicationTheme.Dark;
			isDark = true;
		}

		if (Application.Current != null) {
			foreach (WindowBase window in Application.Current.Windows.OfType<WindowBase>()) {
				WindowFrameTheme.Apply(window.WindowHandle, isDark);
			}
		}

		SetCustomResources(isDark);

		ThemeChangedEventArgs args = new(isDark);
		eventAggregator.GetEvent<ThemeChangedEvent>().Publish(args);
		ThemeChanged?.Invoke(this, args);
	}
}
