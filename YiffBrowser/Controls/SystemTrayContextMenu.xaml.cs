using BaseFramework.Interfaces;
using BaseFramework.Services;
using RW.Base.WPF.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.Controls;

public partial class SystemTrayContextMenu : ContextMenu {
	private readonly ISystemTrayIconService systemTrayIconService;

	public SystemTrayContextMenu(ISystemTrayIconService systemTrayIconService) {
		this.systemTrayIconService = systemTrayIconService ?? throw new ArgumentNullException(nameof(systemTrayIconService));
		InitializeComponent();
	}

	private void DisableTrayIcon_Click(object sender, RoutedEventArgs e) {
		systemTrayIconService.Disable();
		IAppSettingsService appSettingsService = IoC.GetService<IAppSettingsService>();
		appSettingsService.Model.EnableTrayIcon = false;
		appSettingsService.SaveSettings();

		systemTrayIconService.ActivateWindow2();
	}

	private void Exit_Click(object sender, RoutedEventArgs e) {
		App.Instance.Dispatcher.Invoke(() => {
			App.Instance.AskExit();
		});
	}
}
