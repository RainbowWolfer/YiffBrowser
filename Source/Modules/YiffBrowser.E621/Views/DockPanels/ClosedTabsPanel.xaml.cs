using YiffBrowser.BaseFramework.Attributes;
using DevExpress.Mvvm;
using HandyControl.Tools.Extension;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views.DockPanels;

public partial class ClosedTabsPanel : UserControl {
	public ClosedTabsPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(ClosedTabsPanelItem), remainInstance: true)]
internal class ClosedTabsPanelItem : DockPanelItemBase<ClosedTabsPanel> {
	public ClosedTabsPanelItem() {
		Name = "Closed Tabs";
		Icon = "\uE7A7";
	}

	public override void Focus() {
		base.Focus();
		if (Content.DataContext is ClosedTabsPanelViewModel vm) {
			vm.RefreshCommand.Execute(null);
		}
	}
}

internal class ClosedTabsPanelViewModel : DockPanelViewModelBase<ClosedTabsPanelItem> {

	public ObservableCollection<ClosedTabGridRow> RecordList { get; } = [];

	public int MaxPageCount {
		get => GetProperty(() => MaxPageCount);
		set => SetProperty(() => MaxPageCount, value);
	}

	public int PageIndex {
		get => GetProperty(() => PageIndex);
		set {
			SetProperty(() => PageIndex, value);
			Refresh();
		}
	}

	private const int PageSize = 50;
	private bool isListening;

	protected override void OnInitialized() {
		base.OnInitialized();
		EnsureListening();
		PageIndex = 1;
	}

	public override void Dispose() {
		if (isListening) {
			MainViewModel.RecentClosedTabsChanged -= MainViewModel_RecentClosedTabsChanged;
			isListening = false;
		}

		base.Dispose();
	}

	private void EnsureListening() {
		if (isListening) {
			return;
		}

		MainViewModel.RecentClosedTabsChanged += MainViewModel_RecentClosedTabsChanged;
		isListening = true;
	}

	private void MainViewModel_RecentClosedTabsChanged(object? sender, EventArgs e) {
		Refresh();
	}

	private DelegateCommand? refreshCommand;
	public IDelegateCommand RefreshCommand => refreshCommand ??= new(Refresh);
	private void Refresh() {
		EnsureListening();

		List<ClosedTabGridRow> allRows = MainViewModel.GetClosedTabGridRows();
		int total = allRows.Count;
		MaxPageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));

		int page = Math.Clamp(PageIndex <= 0 ? 1 : PageIndex, 1, MaxPageCount);
		if (page != PageIndex) {
			SetProperty(() => PageIndex, page);
		}

		RecordList.Clear();
		RecordList.AddRange(allRows.Skip((page - 1) * PageSize).Take(PageSize));
	}

	private DelegateCommand<ClosedTabGridRow>? restoreRecordCommand;
	public IDelegateCommand RestoreRecordCommand => restoreRecordCommand ??= new(RestoreRecord, CanRestoreRecord);
	private void RestoreRecord(ClosedTabGridRow row) {
		if (CanRestoreRecord(row)) {
			MainViewModel.RestoreClosedTabCommand.Execute(row.Record);
		}
	}
	private bool CanRestoreRecord(ClosedTabGridRow row) => row?.Record != null;

	private DelegateCommand<ClosedTabGridRow>? restoreBatchCommand;
	public IDelegateCommand RestoreBatchCommand => restoreBatchCommand ??= new(RestoreBatch, CanRestoreBatch);
	private void RestoreBatch(ClosedTabGridRow row) {
		if (CanRestoreBatch(row)) {
			MainViewModel.RestoreClosedBatchCommand.Execute(row.Batch);
		}
	}
	private bool CanRestoreBatch(ClosedTabGridRow row) => row?.Batch is { Tabs.Count: > 1 };
}
