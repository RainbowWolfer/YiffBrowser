using YiffBrowser.BaseFramework.Services;
using RW.Base.WPF.Events;

namespace YiffBrowser.BaseFramework.Events;

public class AppSettingsChangedEvent : PubSubEvent<AppSettingsChangedEventArgs>;

public class AppSettingsChangedEventArgs(AppSettingsModel model) : EventArgs {
	public AppSettingsModel Model { get; } = model;
}