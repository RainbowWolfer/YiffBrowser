using RW.Base.WPF.Extensions;
using System.Windows.Controls;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

public partial class E621MainView : UserControl {
	public E621MainView(ViewParameter viewParameter) {
		InitializeComponent();

		E621MainViewModel viewModel = IoC.Resolve<E621MainViewModel>()!;
		viewModel.Initialize(viewParameter);
		DataContext = viewModel;
	}
}