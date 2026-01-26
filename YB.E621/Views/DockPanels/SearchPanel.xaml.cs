using BaseFramework.Attributes;
using BaseFramework.Events;
using RW.Base.WPF.Events;
using System.Windows.Controls;
using YB.E621.Interfaces;
using YB.E621.Services;
using YB.E621.ViewModels;

namespace YB.E621.Views.DockPanels;

public partial class SearchPanel : UserControl {
	public SearchPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(SearchPanelItem), remainInstance: true)]
internal class SearchPanelItem : DockPanelItemBase<SearchPanel> {
	public SearchPanelItem() {
		Name = "Search";
	}
}

internal class SearchPanelViewModel(IEventAggregator eventAggregator) : DockPanelViewModelBase<SearchPanelItem> {


	//public string SearchText {
	//	get => GetProperty(() => SearchText) ?? string.Empty;
	//	set {
	//		SetProperty(() => SearchText, value);
	//		OnSearchTextChanged();
	//	}
	//}




	public E621API? Api { get; private set; }




	protected override void OnInitialized() {
		base.OnInitialized();
		Api = E621API.GetAPI(ViewParameter.ModuleType);
		eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(OnThemeChanged);
	}

	private void OnThemeChanged(ThemeChangedEventArgs args) {
	
	}


}