using BaseFramework.Interfaces;
using BaseFramework.Views;
using DevExpress.Mvvm;
using RW.Base.WPF.Interfaces;
using YB.E621.Parameters;

namespace YB.E621.Views;

public partial class E621MainWindow : WindowBase, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();
		Root.Child = new E621MainView(viewParameter);
	}
}

public class E621MainWindowViewModel() : ViewModelBase {

}
