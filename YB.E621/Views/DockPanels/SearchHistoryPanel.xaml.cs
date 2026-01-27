using BaseFramework.Attributes;
using BaseFramework.ViewModels;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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


	public int PageIndex {
		get => GetProperty(() => PageIndex);
		set {
			SetProperty(() => PageIndex, value);
			Refresh();
		}
	}

	public LoadingStatusViewModel LoadingStatus { get; } = new();

	protected override void OnInitialized() {
		base.OnInitialized();


		for (int i = 0; i < 1000; i++) {
			DataList.Add("");
		}


		PageIndex = 1;
	}


	private AsyncCommand? refreshCommand;
	public IDelegateCommand RefreshCommand => refreshCommand ??= new(Refresh, CanRefresh);
	private async Task Refresh() {
		if (CanRefresh()) {
			LoadingStatus.InitialLoading();
			try {
				await Task.Delay(3000);
				LoadingStatus.DoneLoading();
			} catch (Exception ex) {
				LoadingStatus.LoadingError(ex.Message);
			} finally {

			}
		}
	}
	private bool CanRefresh() => !LoadingStatus.ShowLoading;



	private DelegateCommand? gotoSearchPanelCommand;
	public IDelegateCommand GotoSearchPanelCommand => gotoSearchPanelCommand ??= new(GotoSearchPanel);
	private void GotoSearchPanel() {
		MainViewModel.ShowSearchPanel();
	}

}