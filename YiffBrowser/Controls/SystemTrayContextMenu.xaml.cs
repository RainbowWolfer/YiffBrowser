using BaseFramework.Interfaces;
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
	}

	private void Exit_Click(object sender, RoutedEventArgs e) {

	}
}
