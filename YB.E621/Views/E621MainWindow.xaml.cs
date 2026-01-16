using BaseFramework;
using BaseFramework.Controls;
using BaseFramework.Interfaces;
using DevExpress.Mvvm;
using YB.E621.Parameters;

namespace YB.E621.Views;

public partial class E621MainWindow : WindowBase, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();
		Title = $"{AppConfig.DisplayAppName} - {viewParameter.ModuleType}";
		Root.Child = new E621MainView(viewParameter);
	}
}

public class E621MainWindowViewModel() : ViewModelBase {

}
