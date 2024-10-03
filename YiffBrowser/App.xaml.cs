using BaseFramework.Enums;
using BaseFramework.Interfaces;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using YB.E621.Views;

namespace YiffBrowser {
	public partial class App : Application {

		public E621MainWindowViewModel Window_E621 { get; } = new(ModuleType.E621);
		//public E621MainWindowViewModel Window_E926 { get; } = new(ModuleType.E926);
		//public E621MainWindowViewModel Window_E6AI { get; } = new(ModuleType.E6AI);

		public App() {
			
		}

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			(Window_E621.View as IWindowBase).Window.Show();

		}
	}

}
