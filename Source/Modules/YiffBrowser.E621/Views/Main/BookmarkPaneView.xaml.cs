using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

public partial class BookmarkPaneView : UserControl {
	public BookmarkPaneView() {
		InitializeComponent();
	}

	private void BookmarkTree_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
		if (DataContext is E621MainViewModel vm && vm.OpenBookmarkCommand.CanExecute(null)) {
			vm.OpenBookmarkCommand.Execute(null);
		}
	}

	private void BookmarkTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
		if (DataContext is E621MainViewModel vm) {
			vm.SelectedBookmark = e.NewValue as BookmarkNode;
		}
	}
}