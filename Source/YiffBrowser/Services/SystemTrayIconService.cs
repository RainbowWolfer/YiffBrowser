using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Common.WPF.Controls;
using RW.Common.WPF.Extensions;
using System.Windows;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework;
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.Controls;
using YiffBrowser.Resources.Icons;

namespace YiffBrowser.Services;

internal class SystemTrayIconService : ISystemTrayIconService {

	private NotifyIcon? notifyIcon;

	public bool IsEnabled => notifyIcon != null;

	public void Initialize() {
		IEventAggregator eventAggregator = IoC.EventAggregator;
		eventAggregator.GetEvent<AppSettingsChangedEvent>().Subscribe(OnAppSettingsChanged);
	}

	private void OnAppSettingsChanged(AppSettingsChangedEventArgs args) {
		if (args.Model.EnableTrayIcon) {
			Enable();
		} else {
			Disable();
		}
	}

	public void Enable() {
		if (notifyIcon != null) {
			return;
		}

		notifyIcon = new NotifyIcon {
			Token = AppConfig.NotifyIconToken,
			Text = AppConfig.DisplayAppName,
			IsBlink = false,
			Visibility = Visibility.Visible,
			Icon = new BitmapImage(new Uri(IconResources.YiffBrowserIcon_PNG)),
			ContextMenu = new SystemTrayContextMenu(this),
		};
		notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
		notifyIcon.Initialize();
	}

	private void NotifyIcon_MouseDoubleClick(object sender, RoutedEventArgs e) {
		ActivateWindow();
	}

	public void ActivateWindow() {
		IEnumerable<Window> windows = App.Instance.GetMainWindows();
		if (windows.FirstOrDefault(x => x.Visibility is Visibility.Visible && x.IsVisible) is { } activeWindow) {
			activeWindow.ShowAndActivate();
		} else {
			windows.FirstOrDefault()?.ShowAndActivate();
		}
	}

	public void ActivateWindow2() {
		IEnumerable<Window> windows = App.Instance.GetMainWindows();
		if (windows.FirstOrDefault(x => x.Visibility is Visibility.Visible && x.IsVisible) is not { } activeWindow) {
			windows.FirstOrDefault()?.ShowAndActivate();
		}
	}

	public void Disable() {
		notifyIcon?.Dispose();
		notifyIcon = null;
	}
}
