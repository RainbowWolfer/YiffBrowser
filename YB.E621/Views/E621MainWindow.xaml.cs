using BaseFramework;
using BaseFramework.Controls;
using BaseFramework.Enums;
using BaseFramework.Interfaces;
using ControlzEx;
using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Media.Imaging;
using YB.E621.Parameters;

namespace YB.E621.Views;

public partial class E621MainWindow : WindowBase, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();

		Title = $"{AppConfig.DisplayAppName} - {viewParameter.ModuleType}";

		Root.Child = new E621MainView(viewParameter);

		string url = viewParameter.ModuleType switch {
			ModuleType.E621 => "pack://application:,,,/BaseFramework;component/Resources/Icons/E621Icon.ico",
			ModuleType.E6AI => "pack://application:,,,/BaseFramework;component/Resources/Icons/E6AI.ico",
			ModuleType.E926 => "pack://application:,,,/BaseFramework;component/Resources/Icons/E621Icon.ico",
			_ => "",
		};
		if (url.IsNotBlank()) {
			Icon = new BitmapImage(new Uri(url));
		}

	}
}

public class E621MainWindowViewModel() : ViewModelBase {

}
