using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.Models.Apps;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using YB.E621.Services;
using YB.E621.Views;

namespace YiffBrowser {
	public partial class App : Application {

		public E621MainWindowViewModel Window_E621 { get; } = new(ModuleType.E621);
		//public E621MainWindowViewModel Window_E926 { get; } = new(ModuleType.E926);
		//public E621MainWindowViewModel Window_E6AI { get; } = new(ModuleType.E6AI);

		public App() {

		}

		//private static async void TestLoop() {
		//	while (true) {
		//		if (Current.MainWindow != null) {
		//			Debug.WriteLine($"{FocusManager.GetFocusedElement(Current.MainWindow)} - {Keyboard.FocusedElement}");
		//		} else {
		//			Debug.WriteLine($"null - {Keyboard.FocusedElement}");
		//		}
		//		await Task.Delay(500);
		//	}
		//}

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			AppProfile.Load();

			(Window_E621.View as IWindowBase).Window.Show();

			//TestLoop();
		}

	}

}
