using RW.Base.WPF.Events;

namespace YiffBrowser.BaseFramework.Events;

public class ThemeChangedEvent : PubSubEvent<ThemeChangedEventArgs>;

public class ThemeChangedEventArgs(bool isDarkTheme) : EventArgs {
	public bool IsDarkTheme { get; } = isDarkTheme;
}
