using BaseFramework.Attributes;
using BaseFramework.Enums;
using BaseFramework.ViewModels;
using DevExpress.Mvvm;
using HandyControl.Tools.Extension;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls;
using YB.E621.Interfaces;
using YB.E621.Models.Database;
using YB.E621.Services;
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

internal class SearchHistoryPanelViewModel(
	ISearchRecordHistoryService searchRecordHistoryService
) : DockPanelViewModelBase<SearchHistoryPanelItem> {
	public ObservableCollection<SearchTagsRecord> RecordList { get; } = [];
	public ObservableCollection<SearchTagsRecord> SelectedRecordList { get; } = [];

	public int MaxPageCount {
		get => GetProperty(() => MaxPageCount);
		set => SetProperty(() => MaxPageCount, value);
	}

	public int PageIndex {
		get => GetProperty(() => PageIndex);
		set {
			if (LoadingStatus.ShowLoading) {
				RaisePropertyChanged(() => PageIndex);
				return;
			}
			SetProperty(() => PageIndex, value);
			_ = Refresh();
		}
	}

	public string SearchCondition {
		get => GetProperty(() => SearchCondition);
		set => SetProperty(() => SearchCondition, value);
	}

	public LoadingStatusViewModel LoadingStatus { get; } = new();

	private const int PageSize = 100;

	protected override void OnInitialized() {
		base.OnInitialized();

		SelectedRecordList.CollectionChanged += SelectedRecordList_CollectionChanged;

		PageIndex = 1;

	}

	private void SelectedRecordList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		DeleteSelectedCommand.RaiseCanExecuteChanged();
	}

	private AsyncCommand? refreshCommand;
	public IDelegateCommand RefreshCommand => refreshCommand ??= new(Refresh, CanRefresh);
	private async Task Refresh() {
		if (CanRefresh()) {
			RecordList.Clear();

			LoadingStatus.InitialLoading();
			try {
				ModuleType moduleType = ViewParameter.ModuleType;


				IEnumerable<SearchTagsRecord> records;
				(MaxPageCount, records) = await Task.Run(() => {
					return (
						searchRecordHistoryService.GetPageCount(PageSize, moduleType, SearchCondition),
						searchRecordHistoryService.GetPagedRecords(PageIndex, PageSize, moduleType, SearchCondition)
					);
				});

				RecordList.AddRange(records);

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



	private DelegateCommand<SearchTagsRecord>? copyRecordCommand;
	public IDelegateCommand CopyRecordCommand => copyRecordCommand ??= new(CopyRecord, CanCopyRecord);
	private void CopyRecord(SearchTagsRecord item) {
		if (CanCopyRecord(item)) {
			string tags = string.Join(" ", item.Tags);
			tags.CopyToClipboard();
		}
	}
	private bool CanCopyRecord(SearchTagsRecord item) => item != null && item.Tags != null && item.Tags.All(x => x.IsNotBlank());


	private DelegateCommand<SearchTagsRecord>? searchRecordCommand;
	public IDelegateCommand SearchRecordCommand => searchRecordCommand ??= new(SearchRecord, CanSearchRecord);
	private void SearchRecord(SearchTagsRecord item) {
		if (CanSearchRecord(item)) {
			MainViewModel.SearchSubmit(item.Tags);
		}
	}
	private bool CanSearchRecord(SearchTagsRecord item) => item != null && item.Tags != null;




	private DelegateCommand? deleteSelectedCommand;
	public IDelegateCommand DeleteSelectedCommand => deleteSelectedCommand ??= new(DeleteSelected, CanDeleteSelected);
	private void DeleteSelected() {
		if (CanDeleteSelected()) {
			//todo : messagebox ask
			SearchTagsRecord[] temp = [.. SelectedRecordList];
			IEnumerable<long> idList = temp.Select(x => x.ID);

			int deleteCount = searchRecordHistoryService.RemoveRecords(idList, ViewParameter.ModuleType);
			Debug.WriteLine($"Deleted {deleteCount} record(s)");
		}
	}
	private bool CanDeleteSelected() => SelectedRecordList.IsNotEmpty();

}