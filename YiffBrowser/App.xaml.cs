using BaseFramework;
using BaseFramework.Enums;
using BaseFramework.Helpers;
using RW.Base.WPF;
using RW.Base.WPF.Configs;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using YB.E621.Views;

namespace YiffBrowser;

public partial class App : ApplicationBase {
	private static App? instance;
	public static App Instance => instance!;

	private static Window[] MainWindows { get; set; } = [];

	static App() {
		DebugConfig.Print = o => Debug.WriteLine(o);
		DebugConfig.DebuggerBreak = Debugger.Break;
	}

	protected override bool EnablePipeServerStream => false;

	public ModuleNavigationActions? ModuleNavigationActions { get; }

	public E621MainWindow? Window_E621 { get; }
	public E621MainWindow? Window_E926 { get; }
	public E621MainWindow? Window_E6AI { get; }

	public App() {
		instance = this;

		string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string _ = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory(appDirectory);

		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		try {
			ModuleNavigationActions = new ModuleNavigationActions(ShowE621, ShowE6AI, ShowE926);

			Window_E621 = new E621MainWindow(ModuleType.E621, ModuleNavigationActions);
			Window_E926 = new E621MainWindow(ModuleType.E926, ModuleNavigationActions);
			Window_E6AI = new E621MainWindow(ModuleType.E6AI, ModuleNavigationActions);

			MainWindows = [Window_E621, Window_E926, Window_E6AI];
			foreach (Window window in MainWindows) {
				window.Closing += Window_Closing;
			}

		} catch (Exception ex) {
			Fatal(ex);
		}

	}

	//protected override void OnStartup(StartupEventArgs e) {
	//	if (FatalError) {
	//		return;
	//	}
	//	try {
	//		const string APP_NAME = "RainbowWolfer.YiffBrowser";

	//		mutex = new Mutex(true, APP_NAME, out bool createdNew);

	//		if (!createdNew) {
	//			try {
	//				Process currentProcess = Process.GetCurrentProcess();
	//				Process[] possibleProcesses = Process.GetProcessesByName(currentProcess.ProcessName);

	//				//LoggingService.Log($"{currentProcess.Id} ----- {string.Join(", ", possibleProcesses.Select(x => x.Id))}");

	//				foreach (Process process in possibleProcesses) {
	//					if (process.Id != currentProcess.Id) {
	//						IntPtr hWnd = process.MainWindowHandle;
	//						if (WindowExtension.IsIconic(hWnd)) {
	//							WindowExtension.ShowWindow(hWnd, WindowExtension.SW_RESTORE);
	//						}
	//						WindowExtension.SetForegroundWindow(hWnd);
	//						break;
	//					}
	//				}

	//				//app is already running! Exiting the application  
	//				TotalShutdown();
	//				return;

	//			} catch (Exception ex) {
	//				Debug.WriteLine(ex);
	//			}

	//		}

	//		base.OnStartup(e);

	//		AppProfile.Load();

	//		ShowE621();

	//	} catch (Exception ex) {
	//		FatalError = true;
	//		MessageBox.Show(ex.ToString(), "App Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
	//		TotalShutdown();
	//	}
	//}


	protected override void ShowFatalDialog(Exception exception) {
		MessageBox.Show(exception.ToString(), "Fatal Error");
	}

	private void ShowE621() => Window_E621.ActivateWindow();

	private void ShowE6AI() => Window_E6AI.ActivateWindow();

	private void ShowE926() => Window_E926.ActivateWindow();

	private void Window_Closing(object? sender, CancelEventArgs e) {
		//if (ShuttingDown) {
		//	return;
		//}
		//int visibleCount = MainWindows.Count(x => x.Visibility == Visibility.Visible && x.IsVisible);
		//if (visibleCount <= 1) {
		//	//when last window calls close. ask for confirm close
		//	if (MessageBox.Show("Are you sure to quit Yiff Browser?", "Exit Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) {
		//		e.Cancel = true;
		//		return;
		//	}
		//	TotalShutdown();
		//} else {
		//	if (sender is Window window) {
		//		window.Hide();
		//		e.Cancel = true;
		//	}
		//}
	}

	protected override CultureInfo? GetCultureInfo() {
		return null;
	}

	protected override Window? GetMainWindow() => Window_E621;

	protected override AppManager GetAppManager() => new _AppManager();
	protected override DllLoader GetDllLoader() => new _DllLoader();
	protected override IoCInitializer GetIoCInitializer(IApplication application) => new _IoCInitializer(application);
	protected override FolderConfig GetFolderConfig(IAppManager appManager) => new AppFolderConfig(appManager);

	private class _AppManager : AppManager {
		public override string AppName => AppConfig.AppName;
		public override string BuildMode => AppConfig.IsRelease ? "Release" : "Debug";
		public override bool IsRelease => AppConfig.IsRelease;
	}

	private class _DllLoader() : DllLoader() {
		protected override IEnumerable<string> AdditionalSkipSet() {
			yield return "XamlAnimatedGif";
		}

		protected override void AfterInitialized(IReadOnlyDictionary<string, Assembly> pool, IReadOnlyDictionary<string, Type> types) {
			base.AfterInitialized(pool, types);

			Debug.WriteLine(new string('-', 30));

			foreach (KeyValuePair<string, Assembly> entry in pool) {
				Debug.WriteLine($"Assembly: {entry}");
			}

			foreach (KeyValuePair<string, Type> entry in types) {
				Debug.WriteLine($"Type: {entry}");
			}

			Debug.WriteLine(new string('-', 30));
		}
	}

	private class _IoCInitializer(IApplication application) : IoCInitializer(application) {
		private readonly App application = (App)application;

		protected override void InitializeDependencies() {
			base.InitializeDependencies();

			//builder.RegisterInstance(application.AppSettingsService).As<IAppSettingsService>();
		}
	}

}
