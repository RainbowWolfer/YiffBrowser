using BaseFramework.Attributes;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using YB.E621.Interfaces;
using YB.E621.ViewModels;

namespace YB.E621.Views.DockPanels;

public partial class SearchHistoryPanel : UserControl {
	public SearchHistoryPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(SearchHistoryPanelItem), remainInstance: true)]
internal class SearchHistoryPanelItem : DockPanelItemBase<SearchHistoryPanel> {
	public SearchHistoryPanelItem() {
		Name = "Search History";
	}
}

internal class SearchHistoryPanelViewModel() : DockPanelViewModelBase<SearchHistoryPanelItem> {
	public ObservableCollection<string> DataList { get; } = [];

	protected override void OnInitialized() {
		base.OnInitialized();


		for (int i = 0; i < 1000; i++) {
			DataList.Add("");
		}
	}
}