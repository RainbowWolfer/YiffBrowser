using Autofac;
using BaseFramework;
using BaseFramework.Enums;
using BaseFramework.Helpers;
using BaseFramework.Interfaces;
using BaseFramework.Services;
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
using YB.E621.Parameters;
using YB.E621.Views;
using YiffBrowser.Services;

namespace YiffBrowser;

public partial class App : ApplicationBase {
	private static App? instance;
	public static App Instance => instance!;

	private static Window[] MainWindows { get; set; } = [];

	private static readonly Stopwatch startupStopWatch = new();

	static App() {
		DebugConfig.Print = o => Debug.WriteLine(o);
		DebugConfig.DebuggerBreak = Debugger.Break;
		startupStopWatch.Start();
	}

	protected override bool EnablePipeServerStream => false;

	public E621MainWindow? Window_E621 { get; private set; }
	public E621MainWindow? Window_E926 { get; private set; }
	public E621MainWindow? Window_E6AI { get; private set; }

	private AppSettingsService AppSettingsService { get; }
	private AppProfileService AppProfileService { get; }

	private SystemTrayIconService SystemTrayIconService { get; }

	public App() {
		instance = this;

		string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string _ = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory(appDirectory);

		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		//Resources


		AppFolderConfig folderConfig = (AppFolderConfig)FolderConfig;

		AppSettingsService = new AppSettingsService(folderConfig);
		AppSettingsService.LoadSettings();

		AppProfileService = new AppProfileService(folderConfig);
		AppProfileService.LoadSettings();

		SystemTrayIconService = new SystemTrayIconService();
		SystemTrayIconService.Initialize();

		if (AppSettingsService.Model.EnableTrayIcon) {
			SystemTrayIconService.Enable();
		}

	}

	protected override void BeforeTotalShutdown() {
		base.BeforeTotalShutdown();
		SystemTrayIconService.Disable();
	}

	protected override void BeforeLoadingModules() {
		base.BeforeLoadingModules();

		Resources.MergedDictionaries.RemoveAt(2);
		Resources.MergedDictionaries.RemoveAt(2);
	}

	protected override void AfterLoadingModules() {
		base.AfterLoadingModules();

		try {
			ModuleNavigationActions moduleNavigationActions = new(ShowE621, ShowE6AI, ShowE926);

			Window_E621 = new E621MainWindow(new ViewParameter() {
				ModuleType = ModuleType.E621,
				ModuleNavigationActions = moduleNavigationActions,
			});

			Window_E926 = new E621MainWindow(new ViewParameter() {
				ModuleType = ModuleType.E926,
				ModuleNavigationActions = moduleNavigationActions,
			});

			Window_E6AI = new E621MainWindow(new ViewParameter() {
				ModuleType = ModuleType.E6AI,
				ModuleNavigationActions = moduleNavigationActions,
			});

			MainWindows = [Window_E621, Window_E926, Window_E6AI];
			foreach (Window window in MainWindows) {
				window.Closing += Window_Closing;
			}

		} catch (Exception ex) {
			Fatal(ex);
		}

		//MessageBox.Show($"{IntPtr.Size}");
	}

	protected override void Loaded() {
		base.Loaded();


		//new TestWindow().Show();
		ShowE6AI();
		//ShowE621();

		startupStopWatch.Stop();
		Debug.WriteLine($"app started in {startupStopWatch.ElapsedMilliseconds} ms");

		((_AppManager)AppManager).AppStartupTimeSpan = TimeSpan.FromMilliseconds(startupStopWatch.ElapsedMilliseconds);
		//MessageBox.Show($"app started in {startupStopWatch.ElapsedMilliseconds} ms");
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
		if (AppManager.IsShuttingDown) {
			return;
		}
		Window[] mainWindows = [.. Windows.OfType<IMainWindow>().Cast<Window>()];
		int visibleCount = mainWindows.Count(x => x.Visibility == Visibility.Visible && x.IsVisible);
		if (visibleCount <= 1) {
			//when last window calls close. ask for confirm close
			if (MessageBox.Show("Are you sure to quit Yiff Browser?", "Exit Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) {
				e.Cancel = true;
				return;
			}
			TotalShutdown();
		} else {
			if (sender is Window window) {
				window.Hide();
				e.Cancel = true;
			}
		}
	}

	protected override CultureInfo? GetCultureInfo() {
		return null;
	}

	protected override Window? GetMainWindow() => null;

	protected override AppManager GetAppManager() => new _AppManager();
	protected override DllLoader GetDllLoader() => new _DllLoader();
	protected override IoCInitializer GetIoCInitializer(IApplication application) => new _IoCInitializer(application);
	protected override FolderConfig GetFolderConfig(IAppManager appManager) => new AppFolderConfig(appManager);

	private class _AppManager : AppManager {
		public override string AppName => AppConfig.AppName;
		public override string BuildMode => AppConfig.IsRelease ? "Release" : "Debug";
		public override bool IsRelease => AppConfig.IsRelease;

		public TimeSpan AppStartupTimeSpan {
			get => GetProperty(() => AppStartupTimeSpan);
			set => SetProperty(() => AppStartupTimeSpan, value);
		}
	}

	private class _DllLoader() : DllLoader() {
		protected override IEnumerable<string> AdditionalSkipSet() {
			yield return "XamlAnimatedGif";
			yield return "GongSolutions";
			yield return "ControlzEx";
			yield return "Flyleaf";
			yield return "Dragablz";
			yield return "MaterialDesign";
			yield return "SharpGen";
			yield return "Vortice";
			yield return "WpfColorFontDialog";
		}

		public override void Initialize() {
			base.Initialize();

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

			builder.RegisterInstance(application.AppSettingsService).As<IAppSettingsService>();
			builder.RegisterInstance(application.AppProfileService).As<IAppProfileService>();
			builder.RegisterInstance(application.SystemTrayIconService).As<ISystemTrayIconService>();
		}
	}

}
