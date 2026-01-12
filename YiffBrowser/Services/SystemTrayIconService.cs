using BaseFramework;
using BaseFramework.Interfaces;
using RW.Common.WPF.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using YiffBrowser.Controls;

namespace YiffBrowser.Services;

internal class SystemTrayIconService : ISystemTrayIconService {

	private NotifyIcon? notifyIcon;

	public void Initialize() {

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
			Icon = new BitmapImage(new Uri(@"pack://application:,,,/BaseFramework;component/Resources/Icons/YiffBrowserIcon.png")),
			ContextMenu = new SystemTrayContextMenu(this),
		};
		notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
		notifyIcon.Initialize();
	}

	private void NotifyIcon_MouseDoubleClick(object sender, RoutedEventArgs e) {
		notifyIcon?.Dispose();
	}

	public void Disable() {
		notifyIcon?.Dispose();
		notifyIcon = null;
	}
}
