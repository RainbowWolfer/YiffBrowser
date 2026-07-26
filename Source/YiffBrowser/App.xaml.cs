using Autofac;
using RW.Base.WPF;
using RW.Base.WPF.Configs;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using RW.Common.Helpers;
using RW.Common.WPF;
using RW.Common.WPF.MarkupExtensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using YiffBrowser.BaseFramework;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModelServices;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Views;
using YiffBrowser.Services;

namespace YiffBrowser;

public partial class App : ApplicationBase {
	private static App? instance;
	public static App Instance => instance!;

	private static Window[] MainWindows { get; set; } = [];

	private static readonly Stopwatch startupStopWatch = new();

	static App() {
		DebugConfig.Print = o => Debug.WriteLine(o);
		//DebugConfig.DebuggerBreak = Debugger.Break;
		startupStopWatch.Start();

		ControlConfig.DefaultDirectParameter = false;

		ToolTipServiceFix.Apply();
	}

	protected override bool EnablePipeServerStream => false;

	public E621MainWindow? Window_E621 { get; private set; }
	public E621MainWindow? Window_E926 { get; private set; }
	public E621MainWindow? Window_E6AI { get; private set; }

	private AppSettingsService AppSettingsService { get; }
	private AppProfileService AppProfileService { get; }

	private SystemTrayIconService SystemTrayIconService { get; }

	private bool isRestoringWindows;

	public App() {
		instance = this;

		string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string _ = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory(appDirectory);

		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		AppFolderConfig folderConfig = (AppFolderConfig)FolderConfig;

		AppSettingsService = new AppSettingsService(folderConfig);
		AppSettingsService.LoadSettings();

		AppProfileService = new AppProfileService(folderConfig);
		AppProfileService.LoadSettings();

		SystemTrayIconService = new SystemTrayIconService();

		CustomInitialize.Initialize();

		DispatcherUnhandledException += (_, args) => {
			FlushSessionState();
			Debug.WriteLine(args.Exception);
		};
		AppDomain.CurrentDomain.UnhandledException += (_, args) => {
			FlushSessionState();
			Debug.WriteLine(args.ExceptionObject);
		};

		//FocusDebugLoop();
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
		FlushSessionState();
		base.BeforeTotalShutdown();
		SystemTrayIconService.Disable();
	}

	private void FlushSessionState() {
		try {
			CaptureAllWindowStates();
			AppProfileService.FlushSave();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		}

		try {
			IoC.Resolve<IDownloadService>()?.FlushPersistence();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		}
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
				HookWindowStateEvents(window);
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

		RestoreModuleWindows();

		startupStopWatch.Stop();
		Debug.WriteLine($"app started in {startupStopWatch.ElapsedMilliseconds} ms");

		((AppManagerEx)AppManager).AppStartupTimeSpan = TimeSpan.FromMilliseconds(startupStopWatch.ElapsedMilliseconds);
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

	private void HookWindowStateEvents(Window window) {
		window.LocationChanged += (_, _) => ScheduleCaptureWindowStates();
		window.SizeChanged += (_, _) => ScheduleCaptureWindowStates();
		window.StateChanged += (_, _) => ScheduleCaptureWindowStates();
		window.IsVisibleChanged += (_, _) => ScheduleCaptureWindowStates();
		window.Activated += Window_Activated;
	}

	private void Window_Activated(object? sender, EventArgs e) {
		if (isRestoringWindows || sender is not E621MainWindow mainWindow) {
			return;
		}

		AppProfileService.Model.LastFocusedModule = mainWindow.ModuleType.ToString();
		ScheduleCaptureWindowStates();
	}

	private void ScheduleCaptureWindowStates() {
		if (isRestoringWindows || AppManager.IsShuttingDown) {
			return;
		}

		CaptureAllWindowStates();
		AppProfileService.ScheduleSave();
	}

	private void CaptureAllWindowStates() {
		List<ModuleWindowState> states = [];
		foreach (Window window in MainWindows) {
			if (window is E621MainWindow mainWindow) {
				states.Add(CaptureWindowState(mainWindow));
			}
		}

		AppProfileService.Model.Windows = states;
	}

	private static ModuleWindowState CaptureWindowState(E621MainWindow window) {
		Rect restoreBounds = window.RestoreBounds;
		bool useRestore = window.WindowState != WindowState.Normal;
		double left = useRestore ? restoreBounds.Left : window.Left;
		double top = useRestore ? restoreBounds.Top : window.Top;
		double width = useRestore ? restoreBounds.Width : window.Width;
		double height = useRestore ? restoreBounds.Height : window.Height;

		if (width <= 0) {
			width = 1270;
		}

		if (height <= 0) {
			height = 800;
		}

		return new ModuleWindowState {
			Module = window.ModuleType.ToString(),
			IsOpen = window.Visibility == Visibility.Visible,
			Left = left,
			Top = top,
			Width = width,
			Height = height,
			WindowState = window.WindowState.ToString(),
			RestoreLeft = left,
			RestoreTop = top,
			RestoreWidth = width,
			RestoreHeight = height,
		};
	}

	private void RestoreModuleWindows() {
		isRestoringWindows = true;
		try {
			Dictionary<string, ModuleWindowState> savedByModule = AppProfileService.Model.Windows
				.Where(w => w.Module.IsNotBlank())
				.GroupBy(w => w.Module, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);

			E621MainWindow?[] all = [Window_E621, Window_E926, Window_E6AI];
			List<E621MainWindow> openWindows = [];

			foreach (E621MainWindow? window in all) {
				if (window == null) {
					continue;
				}

				string key = window.ModuleType.ToString();
				if (savedByModule.TryGetValue(key, out ModuleWindowState? state)) {
					ApplyWindowGeometry(window, state);
					if (state.IsOpen) {
						window.Show();
						ApplyWindowState(window, state.WindowState);
						openWindows.Add(window);
					}
				}
			}

			string? lastFocused = AppProfileService.Model.LastFocusedModule;
			E621MainWindow? focusWindow = null;
			if (lastFocused.IsNotBlank()) {
				focusWindow = openWindows.FirstOrDefault(w =>
					string.Equals(w.ModuleType.ToString(), lastFocused, StringComparison.OrdinalIgnoreCase));
			}

			if (focusWindow == null && openWindows.Count > 0) {
				focusWindow = openWindows[0];
			}

			if (focusWindow == null) {
				// Always show at least one window so startup does not look like the app failed to open.
				E621MainWindow? fallback = ResolveWindowByModuleName(lastFocused) ?? Window_E6AI;
				if (fallback != null) {
					if (savedByModule.TryGetValue(fallback.ModuleType.ToString(), out ModuleWindowState? state)) {
						ApplyWindowGeometry(fallback, state);
					}

					fallback.ActivateWindow();
					focusWindow = fallback;
				}
			} else {
				focusWindow.Activate();
				focusWindow.Focus();
			}

			if (focusWindow != null) {
				AppProfileService.Model.LastFocusedModule = focusWindow.ModuleType.ToString();
			}
		} finally {
			isRestoringWindows = false;
		}
	}

	private E621MainWindow? ResolveWindowByModuleName(string? moduleName) {
		if (moduleName.IsBlank()) {
			return null;
		}

		return moduleName.ToUpperInvariant() switch {
			"E621" => Window_E621,
			"E926" => Window_E926,
			"E6AI" => Window_E6AI,
			_ => null,
		};
	}

	private static void ApplyWindowGeometry(E621MainWindow window, ModuleWindowState state) {
		window.WindowStartupLocation = WindowStartupLocation.Manual;

		double left = state.RestoreWidth > 0 ? state.RestoreLeft : state.Left;
		double top = state.RestoreHeight > 0 ? state.RestoreTop : state.Top;
		double width = state.RestoreWidth > 0 ? state.RestoreWidth : state.Width;
		double height = state.RestoreHeight > 0 ? state.RestoreHeight : state.Height;

		Rect clamped = WindowBoundsHelper.ClampToVirtualScreen(left, top, width, height);
		window.Left = clamped.Left;
		window.Top = clamped.Top;
		window.Width = clamped.Width;
		window.Height = clamped.Height;
	}

	private static void ApplyWindowState(E621MainWindow window, string? windowStateName) {
		if (!Enum.TryParse(windowStateName, ignoreCase: true, out WindowState windowState)) {
			windowState = WindowState.Normal;
		}

		window.WindowState = windowState;
	}

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
			ScheduleCaptureWindowStates();
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
			"HtmlAgilityPack",
		];

		protected override IEnumerable<string> AdditionalSkipSet() => skipSet;

		private readonly string[] skipTypeNames = [
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

			foreach (string item in skipTypeNames) {
				if (@namespace.StartsWith(item)) {
					return false;
				}
			}

			return base.MatchType(type);
		}

		public override void Initialize() {
			base.Initialize();

		}

		protected override void AfterInitialized(IReadOnlyList<Assembly> assemblies, IReadOnlyList<Type> types) {
			base.AfterInitialized(assemblies, types);

			Debug.WriteLine(new string('-', 30));

			foreach (Assembly assembly in assemblies) {
				Debug.WriteLine($"Assembly: {assembly}");
			}

			foreach (Type type in types) {
				Debug.WriteLine($"Type: {type}");
			}

			Debug.WriteLine(new string('-', 30));
		}
	}

	private class _IoCInitializer(IApplication application) : IoCInitializer(application) {
		private readonly App application = (App)application;

		public override Autofac.IContainer CreateContainer() {
			return base.CreateContainer();
		}

		protected override void InitializeDependencies() {
			base.InitializeDependencies();

			builder.RegisterInstance(application.AppSettingsService).As<IAppSettingsService>();
			builder.RegisterInstance(application.AppProfileService).As<IAppProfileService>();
			builder.RegisterInstance(application.SystemTrayIconService).As<ISystemTrayIconService>();
		}
	}

}
