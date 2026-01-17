using BaseFramework;
using BaseFramework.Interfaces;
using ControlzEx;
using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using YB.E621.Parameters;

namespace YB.E621.Views;

public partial class E621MainWindow : WindowChromeWindow, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();
		UseNativeCaptionButtons = true;
		Title = $"{AppConfig.DisplayAppName} - {viewParameter.ModuleType}";
		Root.Child = new E621MainView(viewParameter);


		CommandBindings.Add(new CommandBinding(System.Windows.SystemCommands.CloseWindowCommand, (s, e) => System.Windows.SystemCommands.CloseWindow(this)));
		CommandBindings.Add(new CommandBinding(System.Windows.SystemCommands.ShowSystemMenuCommand, (s, e) => {
			//todo : 多显示器下有问题，而且本身位置也有问题
			Point point = WindowState == WindowState.Maximized
				? new Point(0, 30)
				: new Point(Left, Top + 30);
			System.Windows.SystemCommands.ShowSystemMenu(this, point);
		}));
	}
}

public class E621MainWindowViewModel() : ViewModelBase {

}
