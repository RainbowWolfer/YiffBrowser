using System.Windows;
using System.Windows.Input;

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
		CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
		CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => SystemCommands.RestoreWindow(this)));
	}

	public static readonly DependencyProperty TitleBarBackgroundProperty =
			DependencyProperty.Register("TitleBarBackground", typeof(System.Windows.Media.Brush), typeof(CustomWindow), new PropertyMetadata(System.Windows.Media.Brushes.Transparent));

	public System.Windows.Media.Brush TitleBarBackground {
		get => (System.Windows.Media.Brush)GetValue(TitleBarBackgroundProperty);
		set => SetValue(TitleBarBackgroundProperty, value);
	}

}
