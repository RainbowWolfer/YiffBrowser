using Autofac;
using BaseFramework;
using BaseFramework.Enums;
using BaseFramework.Helpers;
using BaseFramework.Interfaces;
using BaseFramework.Services;
using BaseFramework.ViewModelServices;
using RW.Base.WPF;
using RW.Base.WPF.Configs;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using RW.Common.WPF;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
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

		ControlConfig.DefaultDirectParameter = false;
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

		FocusDebugLoop();
	}

	private void FocusDebugLoop() {
		Dispatcher.Invoke(async () => {
			while (true) {
				Debug.WriteLine($"KeyboardFocus: {Keyboard.FocusedElement}");
				Window? mainWindow = GetMainWindows().FirstOrDefault(x => x.IsActive);
				if (mainWindow != null) {
					Debug.WriteLine($"FocusManager: {FocusManager.GetFocusedElement(mainWindow)}");
				}

				await Task.Delay(1000);
			}
		});
	}

	protected override IStatusReport InitializeStatusReport() {
		StatusReport statusReport = new();
		return statusReport;
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

		SystemTrayIconService.Initialize();

		if (AppSettingsService.Model.EnableTrayIcon) {
			SystemTrayIconService.Enable();
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

		((AppManagerEx)AppManager).AppStartupTimeSpan = TimeSpan.FromMilliseconds(startupStopWatch.ElapsedMilliseconds);
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

		if (!SystemTrayIconService.IsEnabled) {
			int visibleCount = GetMainWindows().Count(x => x.Visibility == Visibility.Visible && x.IsVisible);
			if (visibleCount <= 1) {
				//when last window calls close. ask for confirm close.
				if (!AskExit()) {
					e.Cancel = true;
				}
				return;
			}
		}

		if (sender is Window window) {
			window.Hide();
			e.Cancel = true;
		}

	}

	public IEnumerable<Window> GetMainWindows() {
		return [.. Windows.OfType<IMainWindow>().Cast<Window>()];
	}

	public bool AskExit() {
		if (new MessageBoxServiceEx().ShowOkCancelQuestion("Are you sure to quit Yiff Browser?")) {
			TotalShutdown();
			return true;
		} else {
			return false;
		}
	}

	protected override CultureInfo? GetCultureInfo() {
		return null;
	}

	protected override Window? GetMainWindow() => null;

	protected override AppManager GetAppManager() => new AppManagerEx();
	protected override DllLoader GetDllLoader() => new _DllLoader();
	protected override IoCInitializer GetIoCInitializer(IApplication application) => new _IoCInitializer(application);
	protected override FolderConfig GetFolderConfig(IAppManager appManager) => new AppFolderConfig(appManager);

	private class _DllLoader() : DllLoader() {
		private readonly string[] skipSet = [
			"XamlAnimatedGif",
			"GongSolutions",
			"ControlzEx",
			"Flyleaf",
			"Dragablz",
			"MaterialDesign",
			"SharpGen",
			"Vortice",
			"WpfColorFontDialog",
			"LiteDB",
		];

		protected override IEnumerable<string> AdditionalSkipSet() => skipSet;

		private readonly string[] skipNames = [
			"BaseFramework.Extensions",
			"BaseFramework.Enums",
			"BaseFramework.Converters",
			"BaseFramework.Controls",
			"BaseFramework.Utilities",
			"BaseFramework.Helpers",
			"BaseFramework.Models",
			"BaseFramework.Animation",
			"BaseFramework.Animation",
			"BaseFramework.Views",
			"BaseFramework.ViewModels",
			"BaseFramework.ViewModelServices",
			"BaseFramework.Resources",
			"BaseFramework.Interfaces",
			"BaseFramework.Events",
			"BaseFramework.Database",
			"YB.E621.Models",
			"YB.E621.Helpers",
			"YB.E621.Converters",
			"YB.E621.Controls",
			"YB.E621.Views",
			"YB.E621.ViewModels",
			"YB.E621.Parameters",
			"YB.E621.Extensions",
		];

		protected override bool MatchType(Type type) {
			string @namespace = type.Namespace ?? string.Empty;

			foreach (string item in skipNames) {
				if (@namespace.StartsWith(item)) {
					return false;
				}
			}

			return base.MatchType(type);
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
