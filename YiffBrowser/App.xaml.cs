using BaseFramework;
using BaseFramework.Enums;
using BaseFramework.Extensions;
using BaseFramework.Helpers;
using BaseFramework.Models.Apps;
using BaseFramework.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Views;

namespace YiffBrowser {
	public partial class App : Application {

		public static bool ShuttingDown { get; private set; } = false;

		public ModuleNavigationActions? ModuleNavigationActions { get; }

		public E621MainWindowViewModel? Window_E621 { get; }
		public E621MainWindowViewModel? Window_E926 { get; }
		public E621MainWindowViewModel? Window_E6AI { get; }

		private static Window[] MainWindows { get; set; } = [];

		private static Mutex? mutex = null;

		private bool FatalError { get; set; } = false;

		public App() {
			DispatcherUnhandledException += App_DispatcherUnhandledException;
			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			try {
				FolderConfig.Initialize();

				LoggingService.Log($"App Initializing");

				AppConfig.Initialize();

				ModuleNavigationActions = new ModuleNavigationActions(ShowE621, ShowE6AI, ShowE926);

				Window_E621 = new E621MainWindowViewModel(ModuleType.E621, ModuleNavigationActions);
				Window_E926 = new E621MainWindowViewModel(ModuleType.E926, ModuleNavigationActions);
				Window_E6AI = new E621MainWindowViewModel(ModuleType.E6AI, ModuleNavigationActions);

				MainWindows = [Window_E621.View, Window_E926.View, Window_E6AI.View];
				foreach (Window window in MainWindows) {
					window.Closing += Window_Closing;
				}

			} catch (Exception ex) {
				FatalError = true;
				MessageBox.Show(ex.ToString(), "App Constructor Error", MessageBoxButton.OK, MessageBoxImage.Error);
				TotalShutdown();
			}

		}

		protected override void OnStartup(StartupEventArgs e) {
			if (FatalError) {
				return;
			}
			try {
				const string APP_NAME = "RainbowWolfer.YiffBrowser";

				mutex = new Mutex(true, APP_NAME, out bool createdNew);

				if (!createdNew) {
					try {
						Process currentProcess = Process.GetCurrentProcess();
						Process[] possibleProcesses = Process.GetProcessesByName(currentProcess.ProcessName);

						//LoggingService.Log($"{currentProcess.Id} ----- {string.Join(", ", possibleProcesses.Select(x => x.Id))}");

						foreach (Process process in possibleProcesses) {
							if (process.Id != currentProcess.Id) {
								IntPtr hWnd = process.MainWindowHandle;
								if (WindowExtension.IsIconic(hWnd)) {
									WindowExtension.ShowWindow(hWnd, WindowExtension.SW_RESTORE);
								}
								WindowExtension.SetForegroundWindow(hWnd);
								break;
							}
						}

						//app is already running! Exiting the application  
						TotalShutdown();
						return;

					} catch (Exception ex) {
						Debug.WriteLine(ex);
					}

				}

				base.OnStartup(e);

				AppProfile.Load();

				ShowE621();

			} catch (Exception ex) {
				FatalError = true;
				MessageBox.Show(ex.ToString(), "App Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
				TotalShutdown();
			}
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			FatalError = true;
			MessageBox.Show($"{e.Exception}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void ShowE621() {
			ActivateWindow(Window_E621!.View);
		}

		private void ShowE6AI() {
			ActivateWindow(Window_E6AI!.View);
		}

		private void ShowE926() {
			ActivateWindow(Window_E926!.View);
		}

		private static void ActivateWindow(Window window) {
			window.Show();
			window.Activate();
			window.Focus();
			window.Dispatcher.Invoke(() => {
				window.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
			}, DispatcherPriority.Loaded);
		}

		private void Window_Closing(object? sender, CancelEventArgs e) {
			if (ShuttingDown) {
				return;
			}
			int visibleCount = MainWindows.Count(x => x.Visibility == Visibility.Visible && x.IsVisible);
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

		protected override void OnExit(ExitEventArgs e) {
			base.OnExit(e);
			TotalShutdown();
		}

		public static void TotalShutdown() {

			Debug.WriteLine("--- Process Ending ---");

			ShuttingDown = true;


			try {
				mutex?.ReleaseMutex();
				mutex?.Dispose();
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}

			foreach (Window window in MainWindows) {
				window.SafeClose();
			}

			Current.Shutdown();

			Debug.WriteLine("--- Process Ended ---");

			Environment.Exit(0);

			Debug.WriteLine("--- Environment Exited ---");
		}

	}

}
