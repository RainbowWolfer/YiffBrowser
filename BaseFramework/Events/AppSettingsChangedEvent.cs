using BaseFramework.Services;
using RW.Base.WPF.Events;

namespace BaseFramework.Events;

public class AppSettingsChangedEvent : PubSubEvent<AppSettingsChangedEventArgs>;

public class AppSettingsChangedEventArgs(AppSettingsModel model) : EventArgs {
	public AppSettingsModel Model { get; } = model;
}