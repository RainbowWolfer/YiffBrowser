using YiffBrowser.BaseFramework.Attributes;
using YiffBrowser.BaseFramework.ViewModels;
using DevExpress.Mvvm;
using HandyControl.Tools.Extension;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Controls;
using RW.Common.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.Views.DockPanels;

public partial class SearchHistoryPanel : UserControl {
	public SearchHistoryPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(SearchHistoryPanelItem), remainInstance: true)]
internal class SearchHistoryPanelItem : DockPanelItemBase<SearchHistoryPanel> {
	public SearchHistoryPanelItem() {
		Name = "Search History";
		Icon = "\uE81C";
	}
}

internal class SearchHistoryPanelViewModel(
	ISearchRecordHistoryService searchRecordHistoryService
) : DockPanelViewModelBase<SearchHistoryPanelItem> {

	private IUIObjectService<CustomPopup> DeleteConfirmPopupService => GetService<ITypedUIObjectService>(nameof(DeleteConfirmPopupService)).As<CustomPopup>();

	public ObservableCollection<SearchTagsRecord> RecordList { get; } = [];
	public ObservableCollection<SearchTagsRecord> SelectedRecordList { get; } = [];
	public ObservableCollection<SearchTagsRecord> RecordListToBeDeleted { get; } = [];

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

	public LoadingStatus LoadingStatus { get; } = new();

	private const int PageSize = 100;

	protected override void OnInitialized() {
		base.OnInitialized();

		SelectedRecordList.CollectionChanged += SelectedRecordList_CollectionChanged;
		RecordListToBeDeleted.CollectionChanged += RecordListToBeDeleted_CollectionChanged;

		PageIndex = 1;

	}

	private void SelectedRecordList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		DeleteSelectedCommand.RaiseCanExecuteChanged();
	}

	private void RecordListToBeDeleted_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		ConfirmDeleteCommand.RaiseCanExecuteChanged();
	}


	private AsyncCommand? refreshCommand;
	public IDelegateCommand RefreshCommand => refreshCommand ??= new(Refresh, CanRefresh);
	private async Task Refresh() {
		if (CanRefresh()) {
			RecordList.Clear();

			LoadingStatus.Initialize();
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

				LoadingStatus.Done();
			} catch (Exception ex) {
				LoadingStatus.Error(ex.Message);
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
			DeleteConfirmPopupService.Object.Show();

			RecordListToBeDeleted.Clear();
			RecordListToBeDeleted.AddRange(SelectedRecordList);
		}
	}
	private bool CanDeleteSelected() => SelectedRecordList.IsNotEmpty();



	private AsyncCommand? confirmDeleteCommand;
	public IDelegateCommand ConfirmDeleteCommand => confirmDeleteCommand ??= new(ConfirmDelete, CanConfirmDelete);
	private async Task ConfirmDelete() {
		if (CanConfirmDelete()) {
			IEnumerable<long> idList = RecordListToBeDeleted.Select(x => x.ID);

			int deleteCount = searchRecordHistoryService.RemoveRecords(idList, ViewParameter.ModuleType);
			Debug.WriteLine($"Deleted {deleteCount} record(s)");

			DeleteConfirmPopupService.Object.Hide();

			await Refresh();
		}
	}
	private bool CanConfirmDelete() => RecordListToBeDeleted.IsNotEmpty();


}