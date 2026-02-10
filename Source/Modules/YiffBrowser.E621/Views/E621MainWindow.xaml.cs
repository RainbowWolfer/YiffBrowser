using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework;
using YiffBrowser.BaseFramework.Controls;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.E621.Parameters;
using YiffBrowser.Resources.Icons;

namespace YiffBrowser.E621.Views;

public partial class E621MainWindow : WindowBase, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();

		Title = $"{AppConfig.DisplayAppName} - {viewParameter.ModuleType}";

		Root.Child = new E621MainView(viewParameter);

		string url = viewParameter.ModuleType switch {
			ModuleType.E621 => IconResources.E621Icon_ICO,
			ModuleType.E6AI => IconResources.E6AI_ICO,
			ModuleType.E926 => IconResources.E621Icon_ICO,
			_ => "",
		};
		if (url.IsNotBlank()) {
			Icon = new BitmapImage(new Uri(url));
		}

	}

}

public class E621MainWindowViewModel() : ViewModelBase {

}
