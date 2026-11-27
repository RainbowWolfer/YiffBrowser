using DevExpress.Mvvm;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Views.DockPanels;

namespace YiffBrowser.E621.ViewModels;

internal partial class E621MainViewModel {
	private DelegateCommand? openTagsManagementPanelCommand;
	public IDelegateCommand OpenTagsManagementPanelCommand => openTagsManagementPanelCommand ??= new(OpenTagsManagementPanel);
	private void OpenTagsManagementPanel() {
		SelectedDockPanelItem = DockPanelManager.ShowSingle<TagsManagementPanelItem>();
		IsDockPanelCollapsed = false;
	}




	private DelegateCommand<IDockPanel>? closeDockPanelCommand;
	public IDelegateCommand CloseDockPanelCommand => closeDockPanelCommand ??= new(CloseDockPanel, CanCloseDockPanel);
	private void CloseDockPanel(IDockPanel dockPanel) {
		if (CanCloseDockPanel(dockPanel)) {
			DockPanelManager.Close(dockPanel);
		}
	}
	private bool CanCloseDockPanel(IDockPanel dockPanel) => dockPanel != null;


	private DelegateCommand? closeAllDockPanelsCommand;
	public IDelegateCommand CloseAllDockPanelsCommand => closeAllDockPanelsCommand ??= new(CloseAllDockPanels);
	private void CloseAllDockPanels() {
		DockPanelManager.CloseAll();
	}

	public void ShowSearchPanel() {
		SelectedDockPanelItem = DockPanelManager.ShowSingle<SearchPanelItem>();
		SearchPopupService.Object.Hide();
		IsDockPanelCollapsed = false;
	}

	public void ShowSearchHistoryPanel() {
		SelectedDockPanelItem = DockPanelManager.ShowSingle<SearchHistoryPanelItem>();
		SearchPopupService.Object.Hide();
		IsDockPanelCollapsed = false;
	}

	public void ShowClosedTabsPanel() {
		SelectedDockPanelItem = DockPanelManager.ShowSingle<ClosedTabsPanelItem>();
		IsDockPanelCollapsed = false;
	}


	private DelegateCommand? collapseDockPanelCommand;
	public IDelegateCommand CollapseDockPanelCommand => collapseDockPanelCommand ??= new(CollapseDockPanel);
	private void CollapseDockPanel() {
		IsDockPanelCollapsed = true;
	}

	private DelegateCommand? expandDockPanelCommand;
	public IDelegateCommand ExpandDockPanelCommand => expandDockPanelCommand ??= new(ExpandDockPanel, CanExpandDockPanel);
	private void ExpandDockPanel() {
		if (CanExpandDockPanel()) {
			IsDockPanelCollapsed = false;
		}
	}
	private bool CanExpandDockPanel() => IsDockPanelCollapsed;


	private DelegateCommand? dockPanelGridSplitterDoubleClickCommand;
	public IDelegateCommand DockPanelGridSplitterDoubleClickCommand => dockPanelGridSplitterDoubleClickCommand ??= new(DockPanelGridSplitterDoubleClick);
	private void DockPanelGridSplitterDoubleClick() {
		IsDockPanelCollapsed = true;
	}


	private DelegateCommand? dockPanelTabItemHeaderDoubleClickCommand;
	public IDelegateCommand DockPanelTabItemHeaderDoubleClickCommand => dockPanelTabItemHeaderDoubleClickCommand ??= new(DockPanelTabItemHeaderDoubleClick);
	private void DockPanelTabItemHeaderDoubleClick() {
		IsDockPanelCollapsed = !IsDockPanelCollapsed;
	}
}
