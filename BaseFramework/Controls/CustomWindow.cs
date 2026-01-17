using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BaseFramework.Controls;

public class CustomWindow : Window {
	static CustomWindow() {
		// 关联样式
		DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
	}

	public CustomWindow() {
		// 绑定系统命令
		CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => SystemCommands.CloseWindow(this)));
		CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => SystemCommands.MaximizeWindow(this)));
		CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
		CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => SystemCommands.RestoreWindow(this)));
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("BtnMinimize") is Button minimize) {
			//minimize.Click += (s, e) => { WindowState = WindowState.Minimized; };
		}

		if (GetTemplateChild("BtnMax") is Button max) {
			//max.Click += (s, e) => { WindowState = WindowState.Maximized; };
		}

		if (GetTemplateChild("BtnRestore") is Button restore) {
			//restore.Click += (s, e) => { WindowState = WindowState.Normal; };
		}

		if (GetTemplateChild("BtnClose") is Button close) {
			//close.Click += (s, e) => { Close(); };
		}
	}

	public static readonly DependencyProperty TitleBarBackgroundProperty =
			DependencyProperty.Register("TitleBarBackground", typeof(Brush), typeof(CustomWindow), new PropertyMetadata(Brushes.Transparent));

	public Brush TitleBarBackground {
		get => (Brush)GetValue(TitleBarBackgroundProperty);
		set => SetValue(TitleBarBackgroundProperty, value);
	}

	//// Win32 AnimateWindow
	//[DllImport("user32.dll", SetLastError = true)]
	//private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	//// DWM 属性
	//[DllImport("dwmapi.dll", PreserveSig = true)]
	//private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	//private const int AW_BLEND = 0x00080000;
	//private const int AW_ACTIVATE = 0x00020000;
	//private const int AW_HIDE = 0x00010000;
	//private const int AW_CENTER = 0x00000010;

	//protected override void OnSourceInitialized(EventArgs e) {
	//	base.OnSourceInitialized(e);

	//	var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

	//	// 确保系统动画开启
	//	int value = 0; // 0 = 启用动画
	//	DwmSetWindowAttribute(hwnd, 3, ref value, sizeof(int));

	//	// 打开时淡入动画
	//	AnimateWindow(hwnd, 200, AW_BLEND | AW_ACTIVATE);
	//}

	//protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
	//	var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
	//	// 关闭时淡出动画
	//	AnimateWindow(hwnd, 200, AW_BLEND | AW_HIDE);
	//	base.OnClosing(e);
	//}

	//protected override void OnStateChanged(EventArgs e) {
	//	base.OnStateChanged(e);

	//	var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

	//	if (WindowState == WindowState.Minimized) {
	//		// 最小化时动画
	//		AnimateWindow(hwnd, 150, AW_CENTER | AW_HIDE);
	//	} else if (WindowState == WindowState.Normal) {
	//		// 恢复时动画
	//		AnimateWindow(hwnd, 200, AW_BLEND | AW_ACTIVATE);
	//	}
	//}
}
