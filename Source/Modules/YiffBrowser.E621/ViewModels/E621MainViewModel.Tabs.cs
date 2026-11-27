using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Input;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.ViewModels;

internal partial class E621MainViewModel {
	private DelegateCommand? tabsManageCommand;
	public IDelegateCommand TabsManageCommand => tabsManageCommand ??= new(TabsManage);
	private void TabsManage() {
		ShowTabsManage = true;
		LoadBookmarks();
	}


	private DelegateCommand? backToTabsCommand;
	public IDelegateCommand BackToTabsCommand => backToTabsCommand ??= new(BackToTabs, CanBackToTabs);
	private void BackToTabs() {
		if (CanBackToTabs()) {
			IsTabsManageSelecting = false;
			ShowTabsManage = false;
		}
	}
	private bool CanBackToTabs() => Tabs.IsNotEmpty();

	private DelegateCommand? toggleTabsManageSelectingCommand;
	public IDelegateCommand ToggleTabsManageSelectingCommand => toggleTabsManageSelectingCommand ??= new(ToggleTabsManageSelecting);
	private void ToggleTabsManageSelecting() {
		IsTabsManageSelecting = !IsTabsManageSelecting;
	}

	private DelegateCommand? toggleBookmarksPanelCommand;
	public IDelegateCommand ToggleBookmarksPanelCommand => toggleBookmarksPanelCommand ??= new(ToggleBookmarksPanel);
	private void ToggleBookmarksPanel() {
		ShowBookmarksPanel = !ShowBookmarksPanel;
	}

	private DelegateCommand<PostTabItem>? tabCardClickCommand;
	public IDelegateCommand TabCardClickCommand => tabCardClickCommand ??= new(TabCardClick, CanTabCardClick);
	private void TabCardClick(PostTabItem item) {
		if (!CanTabCardClick(item)) {
			return;
		}

		if (IsTabsManageSelecting) {
			item.IsManageSelected = !item.IsManageSelected;
			return;
		}

		int index = Tabs.IndexOf(item);
		if (index < 0) {
			return;
		}

		TabSelectedIndex = index;
		IsTabsManageSelecting = false;
		ShowTabsManage = false;
	}
	private bool CanTabCardClick(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? cloneTabCommand;
	public IDelegateCommand CloneTabCommand => cloneTabCommand ??= new(CloneTab, CanCloneTab);
	private void CloneTab(PostTabItem item) {
		if (!CanCloneTab(item)) {
			return;
		}

		PostTabItem clone = CloneTabItem(item);
		int index = Tabs.IndexOf(item);
		if (index >= 0 && index < Tabs.Count - 1) {
			Tabs.Insert(index + 1, clone);
			TabSelectedIndex = index + 1;
		} else {
			Tabs.Add(clone);
			TabSelectedIndex = Tabs.Count - 1;
		}
	}
	private bool CanCloneTab(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? refreshTabCommand;
	public IDelegateCommand RefreshTabCommand => refreshTabCommand ??= new(RefreshTab, CanRefreshTab);
	private void RefreshTab(PostTabItem item) {
		if (CanRefreshTab(item)) {
			item.Refresh();
		}
	}
	private bool CanRefreshTab(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? moveTabToFrontCommand;
	public IDelegateCommand MoveTabToFrontCommand => moveTabToFrontCommand ??= new(MoveTabToFront, CanMoveTabToFront);
	private void MoveTabToFront(PostTabItem item) {
		if (!CanMoveTabToFront(item)) {
			return;
		}

		int index = Tabs.IndexOf(item);
		Tabs.Move(index, 0);
		if (!IsTabsManageSelecting) {
			TabSelectedIndex = 0;
		}
	}
	private bool CanMoveTabToFront(PostTabItem item) => item != null && Tabs.IndexOf(item) > 0;


	private DelegateCommand<PostTabItem>? selectTabCardCommand;
	public IDelegateCommand SelectTabCardCommand => selectTabCardCommand ??= new(SelectTabCard, CanSelectTabCard);
	private void SelectTabCard(PostTabItem item) {
		if (!CanSelectTabCard(item)) {
			return;
		}

		IsTabsManageSelecting = true;
		item.IsManageSelected = true;
	}
	private bool CanSelectTabCard(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? appendSearchCommand;
	public IDelegateCommand AppendSearchCommand => appendSearchCommand ??= new(AppendSearch, CanAppendSearch);
	private void AppendSearch(PostTabItem item) {
		if (CanAppendSearch(item)) {

		}
	}
	private bool CanAppendSearch(PostTabItem item) => item is { Kind: PostTabKind.Search };

	private DelegateCommand<PostTabItem>? addTabToBookmarksCommand;
	public IDelegateCommand AddTabToBookmarksCommand => addTabToBookmarksCommand ??= new(AddTabToBookmarks, CanAddTabToBookmarks);
	private void AddTabToBookmarks(PostTabItem item) {
		if (CanAddTabToBookmarks(item)) {
			AddBookmarkFromTab(item, SelectedBookmark);
		}
	}
	private bool CanAddTabToBookmarks(PostTabItem item) => item is { Kind: PostTabKind.Search };

	public ICommand CloseTabCommand => new DelegateCommand<PostTabItem>(CloseTab, CanCloseTab);
	private void CloseTab(PostTabItem item) {
		if (CanCloseTab(item)) {
			CloseTabs([item]);
		}
	}
	private bool CanCloseTab(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? closeAllTabsCommand;
	public IDelegateCommand CloseAllTabsCommand => closeAllTabsCommand ??= new(CloseAllTabs, CanCloseAllTabs);
	private void CloseAllTabs(PostTabItem? item) {
		if (CanCloseAllTabs(item)) {
			CloseTabs([.. Tabs]);
		}
	}
	private bool CanCloseAllTabs(PostTabItem? item) => Tabs.Count > 0;



	private DelegateCommand<PostTabItem>? closeAllBeforeTabsCommand;
	public IDelegateCommand CloseAllBeforeTabsCommand => closeAllBeforeTabsCommand ??= new(CloseAllBeforeTabs, CanCloseAllBeforeTabs);
	private void CloseAllBeforeTabs(PostTabItem item) {
		if (CanCloseAllBeforeTabs(item)) {
			int index = Tabs.IndexOf(item);
			CloseTabs([.. Tabs.Take(index)]);
		}
	}
	private bool CanCloseAllBeforeTabs(PostTabItem item) => item != null && Tabs.IndexOf(item) > 0;



	private DelegateCommand<PostTabItem>? closeAllAfterTabsCommand;
	public IDelegateCommand CloseAllAfterTabsCommand => closeAllAfterTabsCommand ??= new(CloseAllAfterTabs, CanCloseAllAfterTabs);
	private void CloseAllAfterTabs(PostTabItem item) {
		if (CanCloseAllAfterTabs(item)) {
			int index = Tabs.IndexOf(item);
			CloseTabs([.. Tabs.Skip(index + 1)]);
		}
	}
	private bool CanCloseAllAfterTabs(PostTabItem item) => item != null && Tabs.IndexOf(item) >= 0 && Tabs.IndexOf(item) < Tabs.Count - 1;



	private DelegateCommand<PostTabItem>? closeAllOtherTabsCommand;
	public IDelegateCommand CloseAllOtherTabsCommand => closeAllOtherTabsCommand ??= new(CloseAllOtherTabs, CanCloseAllOtherTabs);
	private void CloseAllOtherTabs(PostTabItem item) {
		if (CanCloseAllOtherTabs(item)) {
			CloseTabs([.. Tabs.Where(x => x != item)]);
		}
	}
	private bool CanCloseAllOtherTabs(PostTabItem item) => item != null && Tabs.Count > 1 && Tabs.Contains(item);

	private void CloseTabs(IReadOnlyList<PostTabItem> tabs) {
		if (tabs.Count == 0) {
			return;
		}

		ClosedTabBatch batch = new();
		foreach (PostTabItem tab in tabs) {
			batch.Tabs.Add(new ClosedTabRecord(tab));
			tab.Dispose();
			Tabs.Remove(tab);
		}

		recentClosedBatches.Insert(0, batch);
		TrimStoredClosedBatches();
		NotifyClosedTabsChanged(persist: true);
	}

	private DelegateCommand? viewAllClosedTabsCommand;
	public IDelegateCommand ViewAllClosedTabsCommand => viewAllClosedTabsCommand ??= new(ViewAllClosedTabs);
	private void ViewAllClosedTabs() {
		ShowClosedTabsPanel();
	}

	private DelegateCommand<ClosedTabBatch>? restoreClosedBatchCommand;
	public IDelegateCommand RestoreClosedBatchCommand => restoreClosedBatchCommand ??= new(RestoreClosedBatch, CanRestoreClosedBatch);
	private void RestoreClosedBatch(ClosedTabBatch batch) {
		if (!CanRestoreClosedBatch(batch)) {
			return;
		}

		foreach (ClosedTabRecord record in batch.Tabs) {
			Tabs.Add(CreateTabFromRecord(record));
		}

		TabSelectedIndex = Tabs.Count - 1;
		recentClosedBatches.Remove(batch);
		NotifyClosedTabsChanged(persist: true);
	}
	private bool CanRestoreClosedBatch(ClosedTabBatch batch) => batch?.Tabs.Count > 0;

	private DelegateCommand<ClosedTabRecord>? restoreClosedTabCommand;
	public IDelegateCommand RestoreClosedTabCommand => restoreClosedTabCommand ??= new(RestoreClosedTab, CanRestoreClosedTab);
	private void RestoreClosedTab(ClosedTabRecord record) {
		if (!CanRestoreClosedTab(record)) {
			return;
		}

		Tabs.Add(CreateTabFromRecord(record));
		TabSelectedIndex = Tabs.Count - 1;

		ClosedTabBatch? owningBatch = recentClosedBatches.FirstOrDefault(b => b.Tabs.Contains(record));
		if (owningBatch != null) {
			owningBatch.Tabs.Remove(record);
			if (owningBatch.Tabs.Count == 0) {
				recentClosedBatches.Remove(owningBatch);
			}
		}

		NotifyClosedTabsChanged(persist: true);
	}
	private bool CanRestoreClosedTab(ClosedTabRecord record) => record != null;

	private DelegateCommand? restoreClosedTabsCommand;
	public IDelegateCommand RestoreClosedTabsCommand => restoreClosedTabsCommand ??= new(RestoreClosedTabs, CanRestoreClosedTabs);
	private void RestoreClosedTabs() {
		if (CanRestoreClosedTabs()) {
			RestoreClosedBatch(recentClosedBatches[0]);
		}
	}
	private bool CanRestoreClosedTabs() => recentClosedBatches.Count > 0;

	public List<ClosedTabGridRow> GetClosedTabGridRows() {
		List<ClosedTabGridRow> rows = [];
		foreach (ClosedTabBatch batch in recentClosedBatches) {
			foreach (ClosedTabRecord record in batch.Tabs) {
				rows.Add(new ClosedTabGridRow(batch, record));
			}
		}

		return rows;
	}

	private void NotifyClosedTabsChanged(bool persist) {
		RebuildRecentClosedMenu();
		if (persist) {
			ScheduleClosedTabsSave();
		}

		RecentClosedTabsChanged?.Invoke(this, EventArgs.Empty);
	}

	private void RebuildRecentClosedMenu() {
		RecentClosedMenuItems.Clear();

		RecentClosedMenuItems.Add(new RecentClosedMenuEntry {
			Header = "View All Closed Tabs",
			Command = ViewAllClosedTabsCommand,
		});
		RecentClosedMenuItems.Add(new RecentClosedMenuEntry { IsSeparator = true });

		foreach (ClosedTabBatch batch in recentClosedBatches.Take(MaxRecentClosedMenuItems)) {
			if (batch.Tabs.Count == 1) {
				ClosedTabRecord only = batch.Tabs[0];
				RecentClosedMenuItems.Add(new RecentClosedMenuEntry {
					Header = only.DisplayText,
					Command = RestoreClosedTabCommand,
					Parameter = only,
				});
				continue;
			}

			// Parent is a submenu title only — WPF does not reliably fire Command on parents with children.
			RecentClosedMenuEntry batchEntry = new() {
				Header = $"{batch.Tabs.Count} closed tabs",
			};

			batchEntry.Children.Add(new RecentClosedMenuEntry {
				Header = $"Restore All ({batch.Tabs.Count})",
				Command = RestoreClosedBatchCommand,
				Parameter = batch,
			});
			batchEntry.Children.Add(new RecentClosedMenuEntry { IsSeparator = true });

			foreach (ClosedTabRecord record in batch.Tabs) {
				batchEntry.Children.Add(new RecentClosedMenuEntry {
					Header = record.DisplayText,
					Command = RestoreClosedTabCommand,
					Parameter = record,
				});
			}

			RecentClosedMenuItems.Add(batchEntry);
		}

		RaisePropertyChanged(() => HasRecentClosedTabs);
		restoreClosedTabsCommand?.RaiseCanExecuteChanged();
	}

	private void TrimStoredClosedBatches() {
		while (recentClosedBatches.Count > MaxStoredClosedBatches) {
			recentClosedBatches.RemoveAt(recentClosedBatches.Count - 1);
		}
	}

	private void LoadClosedTabs() {
		if (ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		recentClosedBatches.Clear();

		ModuleClosedTabsState? saved = appProfileService.Model.ModuleClosedTabs
			.FirstOrDefault(m => string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase));
		if (saved?.Batches is { Count: > 0 }) {
			foreach (ClosedTabBatchState batchState in saved.Batches) {
				ClosedTabBatch batch = ClosedTabBatch.FromState(batchState);
				if (batch.Tabs.Count > 0) {
					recentClosedBatches.Add(batch);
				}
			}
		}

		TrimStoredClosedBatches();
		closedTabsLoaded = true;
		NotifyClosedTabsChanged(persist: false);
	}

	private void ScheduleClosedTabsSave() {
		if (!closedTabsLoaded || ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		ModuleClosedTabsState state = new() {
			Module = module,
			Batches = recentClosedBatches.Select(b => b.ToState()).ToList(),
		};

		List<ModuleClosedTabsState> all = appProfileService.Model.ModuleClosedTabs
			.Where(m => !string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase))
			.ToList();
		all.Add(state);
		appProfileService.Model.ModuleClosedTabs = all;
		appProfileService.ScheduleSave();
	}
}
