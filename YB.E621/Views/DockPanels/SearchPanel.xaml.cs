using BaseFramework.Attributes;
using System.Windows.Controls;
using YB.E621.Interfaces;
using YB.E621.ViewModels;

namespace YB.E621.Views.DockPanels;

public partial class SearchPanel : UserControl {
	public SearchPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(SearchPanelItem), remainInstance: true)]
public class SearchPanelItem : DockPanelItemBase<SearchPanel> {
	public SearchPanelItem() {
		Name = "Search";
	}
}

internal class SearchPanelViewModel : DockPanelViewModelBase<SearchPanelItem> {

}