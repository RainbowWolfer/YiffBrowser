using DevExpress.Mvvm;
using GongSolutions.Wpf.DragDrop;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModelServices;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;
using YiffBrowser.E621.Views.DockPanels;

namespace YiffBrowser.E621.Views;

public partial class E621MainView : UserControl {
	public E621MainView(ViewParameter viewParameter) {
		InitializeComponent();

		E621MainViewModel viewModel = IoC.Resolve<E621MainViewModel>()!;
		viewModel.Initialize(viewParameter);
		DataContext = viewModel;

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

internal static class E621MainViewExtension {


	public static bool? GetDockPanelShow(DependencyObject obj) => (bool?)obj.GetValue(DockPanelShowProperty);
	public static void SetDockPanelShow(DependencyObject obj, bool? value) => obj.SetValue(DockPanelShowProperty, value);
	public static readonly DependencyProperty DockPanelShowProperty = DependencyProperty.RegisterAttached(
		"DockPanelShow",
		typeof(bool?),
		typeof(E621MainViewExtension),
		new PropertyMetadata(null, OnDockPanelShowChanged)
	);
	private static void OnDockPanelShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		Update(d);
	}


	public static bool GetIsDockPanelCollapsed(DependencyObject obj) => (bool)obj.GetValue(IsDockPanelCollapsedProperty);
	public static void SetIsDockPanelCollapsed(DependencyObject obj, bool value) => obj.SetValue(IsDockPanelCollapsedProperty, value);
	public static readonly DependencyProperty IsDockPanelCollapsedProperty = DependencyProperty.RegisterAttached(
		"IsDockPanelCollapsed",
		typeof(bool),
		typeof(E621MainViewExtension),
		new PropertyMetadata(false, OnIsDockPanelCollapsedChanged)
	);
	private static void OnIsDockPanelCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		Update(d);
	}

	private static void Update(DependencyObject d) {

		if (d is RowDefinition rowDefinition) {
			if (GetDockPanelShow(d) is true) {
				if (GetIsDockPanelCollapsed(d) is true) {
					rowDefinition.MinHeight = 0;
					rowDefinition.Height = new GridLength(1, GridUnitType.Auto);
				} else {
					rowDefinition.MinHeight = 100;
					rowDefinition.Height = new GridLength(300, GridUnitType.Pixel);
				}
			} else {
				rowDefinition.MinHeight = 0;
				rowDefinition.Height = new GridLength(0, GridUnitType.Pixel);
			}
		}
	}
}

internal class E621MainViewModel(
	IApplication application,
	IAppManager appManager,
	IThemeManager themeManager,
	ISearchRecordHistoryService searchRecordHistoryService,
	IViewConfigService viewConfigService,
	IDownloadService downloadService,
	IAppProfileService appProfileService
) : ViewModelBase {

	public IDownloadService DownloadService { get; } = downloadService;

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControl => GetService<ITypedUIObjectService>(nameof(UserControl)).As<UserControl>();

	public IUIObjectService<ButtonPopup> SearchPopupService => GetService<ITypedUIObjectService>(nameof(SearchPopupService)).As<ButtonPopup>();
	public IUIObjectService<ButtonPopup> SitePopupService => GetService<ITypedUIObjectService>(nameof(SitePopupService)).As<ButtonPopup>();

	public IDialogServiceEx AppSettingsDialog => GetService<IDialogServiceEx>(nameof(AppSettingsDialog));
	public IDialogServiceEx DownloadDialog => GetService<IDialogServiceEx>(nameof(DownloadDialog));

	public IAppManager AppManager { get; } = appManager;
	public IViewConfigService ViewConfigService { get; } = viewConfigService;
	public ViewParameter? ViewParameter { get; private set; }

	public ObservableCollection<PostTabItem> Tabs { get; } = [];

	private bool isRestoringSession;
	private bool downloadFeedbackSubscribed;
	private bool bookmarksLoaded;
	private DispatcherTimer? downloadIconFeedbackTimer;
	private DispatcherTimer? downloadToastTimer;

	public bool IsDownloadButtonFeedbackActive {
		get => GetProperty(() => IsDownloadButtonFeedbackActive);
		set => SetProperty(() => IsDownloadButtonFeedbackActive, value);
	}

	public bool ShowDownloadAddedToast {
		get => GetProperty(() => ShowDownloadAddedToast);
		set => SetProperty(() => ShowDownloadAddedToast, value);
	}

	public string DownloadAddedToastText {
		get => GetProperty(() => DownloadAddedToastText);
		set => SetProperty(() => DownloadAddedToastText, value);
	}

	public bool IsTabsManageSelecting {
		get => GetProperty(() => IsTabsManageSelecting);
		set {
			bool previous = GetProperty(() => IsTabsManageSelecting);
			SetProperty(() => IsTabsManageSelecting, value);
			if (previous && !value) {
				foreach (PostTabItem tab in Tabs) {
					tab.IsManageSelected = false;
				}
			}
		}
	}

	public bool ShowBookmarksPanel {
		get => GetProperty(() => ShowBookmarksPanel);
		set {
			bool previous = GetProperty(() => ShowBookmarksPanel);
			SetProperty(() => ShowBookmarksPanel, value);
			if (previous != value) {
				RaisePropertyChanged(() => BookmarksPanelWidth);
			}
		}
	}

	public GridLength BookmarksPanelWidth => ShowBookmarksPanel ? new GridLength(260) : new GridLength(0);

	public ObservableCollection<BookmarkNode> BookmarkRoots { get; } = [];

	public ObservableCollection<RecentClosedMenuEntry> RecentClosedMenuItems { get; } = [];

	private readonly List<ClosedTabBatch> recentClosedBatches = [];
	private const int MaxRecentClosedMenuItems = 30;
	private const int MaxStoredClosedBatches = 500;
	private bool closedTabsLoaded;

	public event EventHandler? RecentClosedTabsChanged;

	public bool HasRecentClosedTabs => recentClosedBatches.Count > 0;

	public BookmarkNode? SelectedBookmark {
		get => GetProperty(() => SelectedBookmark);
		set {
			SetProperty(() => SelectedBookmark, value);
			openBookmarkCommand?.RaiseCanExecuteChanged();
			deleteBookmarkCommand?.RaiseCanExecuteChanged();
		}
	}

	public IDropTarget BookmarkDropHandler { get; private set; } = null!;

	public E621UserService UserService {
		get => GetProperty(() => UserService);
		private set => SetProperty(() => UserService, value);
	}


	public bool ShowTabsManage {
		get => GetProperty(() => ShowTabsManage);
		set => SetProperty(() => ShowTabsManage, value);
	}

	public int TabSelectedIndex {
		get => GetProperty(() => TabSelectedIndex);
		set {
			int previous = GetProperty(() => TabSelectedIndex);
			SetProperty(() => TabSelectedIndex, value);
			if (previous != value) {
				ScheduleSessionSave();
			}
		}
	}

	public bool IsLoggedIn {
		get => GetProperty(() => IsLoggedIn);
		set => SetProperty(() => IsLoggedIn, value);
	}

	public IDockPanel SelectedDockPanelItem {
		get => GetProperty(() => SelectedDockPanelItem);
		set {
			SetProperty(() => SelectedDockPanelItem, value);
			value?.Focus();
		}
	}

	public DockPanelManager DockPanelManager { get; } = new();

	public bool IsDockPanelCollapsed {
		get => GetProperty(() => IsDockPanelCollapsed);
		set => SetProperty(() => IsDockPanelCollapsed, value);
	}

	public void Initialize(ViewParameter parameter) {
		ViewParameter = parameter;

		Tabs.CollectionChanged += Tabs_CollectionChanged;

		UserService = E621UserService.GetUserService(parameter.ModuleType);
		UserService.LoginChanged += UserService_LoginChanged;

		DockPanelManager.Initialize(application, this);

		BookmarkDropHandler = new TabBookmarkDropHandler(this);
		ShowBookmarksPanel = true;
		LoadBookmarks();
		LoadClosedTabs();

		if (!downloadFeedbackSubscribed) {
			downloadFeedbackSubscribed = true;
			DownloadService.DownloadsQueued += OnDownloadsQueued;
		}
	}

	private void OnDownloadsQueued(object? sender, int count) {
		if (count <= 0) {
			return;
		}

		void Show() {
			// Retrigger DataTrigger storyboards when feedback is already visible.
			IsDownloadButtonFeedbackActive = false;
			ShowDownloadAddedToast = false;

			DownloadAddedToastText = count == 1
				? "1 post added to downloads"
				: $"{count} posts added to downloads";
			IsDownloadButtonFeedbackActive = true;
			ShowDownloadAddedToast = true;

			downloadIconFeedbackTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
			downloadIconFeedbackTimer.Tick -= OnDownloadIconFeedbackTimerTick;
			downloadIconFeedbackTimer.Tick += OnDownloadIconFeedbackTimerTick;
			downloadIconFeedbackTimer.Stop();
			downloadIconFeedbackTimer.Start();

			downloadToastTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
			downloadToastTimer.Tick -= OnDownloadToastTimerTick;
			downloadToastTimer.Tick += OnDownloadToastTimerTick;
			downloadToastTimer.Stop();
			downloadToastTimer.Start();
		}

		Dispatcher dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
		if (dispatcher.CheckAccess()) {
			Show();
		} else {
			dispatcher.BeginInvoke(Show);
		}
	}

	private void OnDownloadIconFeedbackTimerTick(object? sender, EventArgs e) {
		downloadIconFeedbackTimer?.Stop();
		IsDownloadButtonFeedbackActive = false;
	}

	private void OnDownloadToastTimerTick(object? sender, EventArgs e) {
		downloadToastTimer?.Stop();
		ShowDownloadAddedToast = false;
	}

	private void Tabs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		BackToTabsCommand.RaiseCanExecuteChanged();
		RaisePropertyChanged(() => Tabs);
		ScheduleSessionSave();
	}

	private PostTabItem CreateTabItem(string[] tags, int initialPage = 1) {
		PostTabItem item = new(this, tags, initialPage);
		return item;
	}

	public void ScheduleSessionSave() {
		if (isRestoringSession || ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		ModuleSessionState session = new() {
			Module = module,
			SelectedTabIndex = TabSelectedIndex,
			Tabs = Tabs.Select(t => new TabSessionState {
				Tags = t.Tags,
				CurrentPage = t.View.CurrentPage,
			}).ToList(),
		};

		List<ModuleSessionState> modules = appProfileService.Model.Modules
			.Where(m => !string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase))
			.ToList();
		modules.Add(session);
		appProfileService.Model.Modules = modules;
		appProfileService.ScheduleSave();
	}

	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		DispatcherService.Dispatcher.BeginInvoke(() => {
			Initialize();
			RestoreOrCreateTabs();
		}, DispatcherPriority.Background);
	}

	private void RestoreOrCreateTabs() {
		if (ViewParameter == null) {
			return;
		}

		isRestoringSession = true;
		try {
			string module = ViewParameter.ModuleType.ToString();
			ModuleSessionState? saved = appProfileService.Model.Modules
				.FirstOrDefault(m => string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase));

			if (saved?.Tabs is { Count: > 0 }) {
				foreach (TabSessionState tab in saved.Tabs) {
					string[] tags = tab.Tags is { Length: > 0 } ? tab.Tags : ["wallpaper", "rating:safe"];
					Tabs.Add(CreateTabItem(tags, Math.Max(1, tab.CurrentPage)));
				}

				TabSelectedIndex = NumberHelper.Clamp(saved.SelectedTabIndex, 0, Tabs.Count - 1);
			} else {
				Tabs.Add(CreateTabItem(["wallpaper", "rating:safe"]));
				TabSelectedIndex = 0;
			}
		} finally {
			isRestoringSession = false;
		}
	}

	private async void Initialize() {
		try {
			if (UserService != null) {
				await UserService.Initialize();
			}
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		}
	}

	//public E621MainWindowViewModel(ModuleType moduleType, ModuleNavigationActions moduleNavigationActions) {
	//	UserService = E621UserService.GetUserService(moduleType);
	//	UserService.LoginChanged += UserService_LoginChanged;

	//	SearchViewModel = new SearchViewModel(moduleType);
	//	SearchViewModel.SearchSubmit += SearchViewModel_SearchSubmit;

	//	UserLoginViewModel = new UserLoginViewModel(moduleType);
	//	UserViewModel = new UserViewModel(moduleType);

	//	ModuleType = moduleType;
	//	ModuleNavigationActions = moduleNavigationActions;
	//	CurrentWindowService.GetWindow().Title = $"Yiff Browser - {moduleType}";

	//	//Tabs.Add(new PostsViewModel(ModuleType, ["order:rank"]));
	//	//Tabs.Add(new PostsViewModel(ModuleType, ["type:gif", "order:filesize"]));
	//	//Tabs.Add(new PostsViewModel(ModuleType, ["type:gif", "order:filesize"]));
	//	Tabs.Add(new PostsViewModel(ModuleType, ["type:gif"]));
	//	//Tabs.Add(new PostsViewModel(ModuleType, ["type:gif"]));
	//	//Tabs.Add(new PostsViewModel(ModuleType, ["type:webm"]));
	//	TabSelectedIndex = 0;
	//}

	public void SearchSubmit(string[] tags) {
		SearchPopupService.Object.Hide();

		if (ViewParameter != null) {

			PostTabItem item = CreateTabItem(tags);
			Tabs.Add(item);
			TabSelectedIndex = Tabs.Count - 1;

			DispatcherService.Dispatcher.BeginInvoke(() => {
				UserControl.Focus();
			}, DispatcherPriority.Loaded);

			searchRecordHistoryService.AddRecord(SearchTagsRecord.Create(tags), ViewParameter.ModuleType);
		}

	}

	private void UserService_LoginChanged(E621User? sender, E621Post? args) {
		IsLoggedIn = sender != null;
	}

	public ICommand ShowE621Command => new DelegateCommand(() => {
		SitePopupService.Object.Hide();
		ViewParameter?.ModuleNavigationActions?.ShowE621?.Invoke();
	});

	public ICommand ShowE6AICommand => new DelegateCommand(() => {
		SitePopupService.Object.Hide();
		ViewParameter?.ModuleNavigationActions?.ShowE6AI?.Invoke();
	});

	public ICommand ShowE926Command => new DelegateCommand(() => {
		SitePopupService.Object.Hide();
		ViewParameter?.ModuleNavigationActions?.ShowE926?.Invoke();
	});



	private DelegateCommand? showAppSettingsDialogCommand;
	public IDelegateCommand ShowAppSettingsDialogCommand => showAppSettingsDialogCommand ??= new(ShowAppSettingsDialog, CanShowAppSettingsDialog);
	private void ShowAppSettingsDialog() {
		if (CanShowAppSettingsDialog()) {
			AppSettingsDialog.ShowOKCancel(this, null);
		}
	}
	private bool CanShowAppSettingsDialog() => true;


	private DelegateCommand? switchThemeCommand;
	public IDelegateCommand SwitchThemeCommand => switchThemeCommand ??= new(SwitchTheme);
	private void SwitchTheme() {
		themeManager.ToggleTheme();
	}




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


	private DelegateCommand? openDownloadCommand;
	public IDelegateCommand OpenDownloadCommand => openDownloadCommand ??= new(OpenDownload);
	private void OpenDownload() {
		DownloadDialog.ShowDialog(this, null);
	}


	private DelegateCommand<PostTabItem>? cloneTabCommand;
	public IDelegateCommand CloneTabCommand => cloneTabCommand ??= new(CloneTab, CanCloneTab);
	private void CloneTab(PostTabItem item) {
		if (!CanCloneTab(item)) {
			return;
		}

		PostTabItem clone = CreateTabItem(item.Tags, item.CurrentPage);
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
	private bool CanAppendSearch(PostTabItem item) => item != null;

	private DelegateCommand<PostTabItem>? addTabToBookmarksCommand;
	public IDelegateCommand AddTabToBookmarksCommand => addTabToBookmarksCommand ??= new(AddTabToBookmarks, CanAddTabToBookmarks);
	private void AddTabToBookmarks(PostTabItem item) {
		if (CanAddTabToBookmarks(item)) {
			AddBookmarkFromTab(item, SelectedBookmark);
		}
	}
	private bool CanAddTabToBookmarks(PostTabItem item) => item != null;

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
			batch.Tabs.Add(new ClosedTabRecord(tab.Tags, tab.CurrentPage));
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
			Tabs.Add(CreateTabItem(record.Tags, record.Page));
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

		Tabs.Add(CreateTabItem(record.Tags, record.Page));
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

	private DelegateCommand<BookmarkNode?>? newBookmarkFolderCommand;
	public IDelegateCommand NewBookmarkFolderCommand => newBookmarkFolderCommand ??= new(NewBookmarkFolder);
	private void NewBookmarkFolder(BookmarkNode? relative) {
		BookmarkNode? parent;
		if (relative != null) {
			parent = relative.IsFolder ? relative : relative.Parent;
		} else {
			parent = SelectedBookmark?.IsFolder == true ? SelectedBookmark : SelectedBookmark?.Parent;
		}

		ObservableCollection<BookmarkNode> siblings = parent?.Children ?? BookmarkRoots;
		string name = NextUniqueName(siblings, "New Folder");
		BookmarkNode folder = BookmarkNode.CreateFolder(name, parent);
		siblings.Add(folder);
		SelectedBookmark = folder;
		ScheduleBookmarksSave();
	}

	private DelegateCommand<BookmarkNode?>? deleteBookmarkCommand;
	public IDelegateCommand DeleteBookmarkCommand => deleteBookmarkCommand ??= new(DeleteBookmark, CanDeleteBookmark);
	private void DeleteBookmark(BookmarkNode? node) {
		BookmarkNode? target = node ?? SelectedBookmark;
		if (!CanDeleteBookmark(target) || target is null) {
			return;
		}

		ObservableCollection<BookmarkNode> siblings = target.Parent?.Children ?? BookmarkRoots;
		siblings.Remove(target);
		if (ReferenceEquals(SelectedBookmark, target)) {
			SelectedBookmark = null;
		}

		ScheduleBookmarksSave();
	}
	private bool CanDeleteBookmark(BookmarkNode? node) => (node ?? SelectedBookmark) != null;

	private DelegateCommand<BookmarkNode?>? openBookmarkCommand;
	public IDelegateCommand OpenBookmarkCommand => openBookmarkCommand ??= new(OpenBookmark, CanOpenBookmark);
	private void OpenBookmark(BookmarkNode? node) {
		BookmarkNode? target = node ?? SelectedBookmark;
		if (!CanOpenBookmark(target) || target?.Tags is null) {
			return;
		}

		PostTabItem item = CreateTabItem(target.Tags, target.Page);
		Tabs.Add(item);
		TabSelectedIndex = Tabs.Count - 1;
		IsTabsManageSelecting = false;
		ShowTabsManage = false;
	}
	private bool CanOpenBookmark(BookmarkNode? node) => (node ?? SelectedBookmark)?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? beginRenameBookmarkCommand;
	public IDelegateCommand BeginRenameBookmarkCommand => beginRenameBookmarkCommand ??= new(BeginRenameBookmark, CanBeginRenameBookmark);
	private void BeginRenameBookmark(BookmarkNode node) {
		if (!CanBeginRenameBookmark(node)) {
			return;
		}

		SelectedBookmark = node;
		CloseAllBookmarkPopups();
		node.DraftName = node.Name;
		DispatcherService.Dispatcher.BeginInvoke(() => {
			node.IsRenamePopupOpen = true;
		}, DispatcherPriority.Input);
	}
	private bool CanBeginRenameBookmark(BookmarkNode node) => node?.IsFolder == true;

	private DelegateCommand<BookmarkNode>? confirmRenameBookmarkCommand;
	public IDelegateCommand ConfirmRenameBookmarkCommand => confirmRenameBookmarkCommand ??= new(ConfirmRenameBookmark, CanConfirmRenameBookmark);
	private void ConfirmRenameBookmark(BookmarkNode node) {
		if (!CanConfirmRenameBookmark(node)) {
			return;
		}

		node.Name = node.DraftName.Trim();
		node.IsRenamePopupOpen = false;
		ScheduleBookmarksSave();
	}
	private bool CanConfirmRenameBookmark(BookmarkNode node) => node?.IsFolder == true && node.DraftName.IsNotBlank();

	private DelegateCommand<BookmarkNode>? cancelRenameBookmarkCommand;
	public IDelegateCommand CancelRenameBookmarkCommand => cancelRenameBookmarkCommand ??= new(CancelRenameBookmark);
	private void CancelRenameBookmark(BookmarkNode node) {
		if (node != null) {
			node.IsRenamePopupOpen = false;
		}
	}

	private DelegateCommand<BookmarkNode>? beginEditBookmarkCommand;
	public IDelegateCommand BeginEditBookmarkCommand => beginEditBookmarkCommand ??= new(BeginEditBookmark, CanBeginEditBookmark);
	private void BeginEditBookmark(BookmarkNode node) {
		if (!CanBeginEditBookmark(node)) {
			return;
		}

		SelectedBookmark = node;
		CloseAllBookmarkPopups();
		node.DraftTagsText = node.Tags is { Length: > 0 } ? string.Join(" ", node.Tags) : string.Empty;
		node.DraftPage = node.Page;
		DispatcherService.Dispatcher.BeginInvoke(() => {
			node.IsEditPopupOpen = true;
		}, DispatcherPriority.Input);
	}
	private bool CanBeginEditBookmark(BookmarkNode node) => node?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? confirmEditBookmarkCommand;
	public IDelegateCommand ConfirmEditBookmarkCommand => confirmEditBookmarkCommand ??= new(ConfirmEditBookmark, CanConfirmEditBookmark);
	private void ConfirmEditBookmark(BookmarkNode node) {
		if (!CanConfirmEditBookmark(node)) {
			return;
		}

		string[] tags = (node.DraftTagsText ?? string.Empty)
			.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		node.Tags = tags;
		node.Page = Math.Max(1, node.DraftPage);
		node.Name = tags.Length > 0 ? string.Join(" ", tags) : "Bookmark";
		node.IsEditPopupOpen = false;
		ScheduleBookmarksSave();
	}
	private bool CanConfirmEditBookmark(BookmarkNode node) => node?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? cancelEditBookmarkCommand;
	public IDelegateCommand CancelEditBookmarkCommand => cancelEditBookmarkCommand ??= new(CancelEditBookmark);
	private void CancelEditBookmark(BookmarkNode node) {
		if (node != null) {
			node.IsEditPopupOpen = false;
		}
	}

	private void CloseAllBookmarkPopups() {
		foreach (BookmarkNode node in EnumerateBookmarkNodes(BookmarkRoots)) {
			node.IsRenamePopupOpen = false;
			node.IsEditPopupOpen = false;
		}
	}

	private static IEnumerable<BookmarkNode> EnumerateBookmarkNodes(IEnumerable<BookmarkNode> roots) {
		foreach (BookmarkNode node in roots) {
			yield return node;
			foreach (BookmarkNode child in EnumerateBookmarkNodes(node.Children)) {
				yield return child;
			}
		}
	}

	public void AddBookmarkFromTab(PostTabItem tab, BookmarkNode? target) {
		BookmarkNode? folder = ResolveFolderTarget(target);
		ObservableCollection<BookmarkNode> siblings = folder?.Children ?? BookmarkRoots;
		BookmarkNode bookmark = BookmarkNode.CreateBookmark(tab.Tags, tab.CurrentPage, parent: folder);
		siblings.Add(bookmark);
		SelectedBookmark = bookmark;
		ScheduleBookmarksSave();
	}

	public void MoveBookmarkNode(BookmarkNode node, BookmarkNode? target, RelativeInsertPosition insertPosition) {
		if (target != null && (ReferenceEquals(node, target) || IsDescendantOf(node, target))) {
			return;
		}

		ObservableCollection<BookmarkNode> oldSiblings = node.Parent?.Children ?? BookmarkRoots;
		oldSiblings.Remove(node);

		if (target == null) {
			node.Parent = null;
			BookmarkRoots.Add(node);
		} else if (target.IsFolder && insertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter)) {
			node.Parent = target;
			target.Children.Add(node);
		} else {
			BookmarkNode? parent = target.Parent;
			ObservableCollection<BookmarkNode> siblings = parent?.Children ?? BookmarkRoots;
			node.Parent = parent;
			int index = siblings.IndexOf(target);
			if (index < 0) {
				siblings.Add(node);
			} else if (insertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem)) {
				siblings.Insert(index, node);
			} else {
				siblings.Insert(Math.Min(index + 1, siblings.Count), node);
			}
		}

		ScheduleBookmarksSave();
	}

	private static BookmarkNode? ResolveFolderTarget(BookmarkNode? target) {
		if (target == null) {
			return null;
		}

		return target.IsFolder ? target : target.Parent;
	}

	private static bool IsDescendantOf(BookmarkNode ancestor, BookmarkNode node) {
		BookmarkNode? current = node;
		while (current != null) {
			if (ReferenceEquals(current, ancestor)) {
				return true;
			}

			current = current.Parent;
		}

		return false;
	}

	private static string NextUniqueName(IEnumerable<BookmarkNode> siblings, string baseName) {
		HashSet<string> names = new(siblings.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
		if (!names.Contains(baseName)) {
			return baseName;
		}

		for (int i = 2; i < 10_000; i++) {
			string candidate = $"{baseName} ({i})";
			if (!names.Contains(candidate)) {
				return candidate;
			}
		}

		return $"{baseName} ({Guid.NewGuid():N})";
	}

	private void LoadBookmarks() {
		if (ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		BookmarkRoots.Clear();

		ModuleBookmarksState? saved = appProfileService.Model.ModuleBookmarks
			.FirstOrDefault(m => string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase));
		if (saved?.Roots is { Count: > 0 }) {
			foreach (BookmarkNodeState root in saved.Roots) {
				BookmarkRoots.Add(BookmarkNode.FromState(root));
			}
		}

		bookmarksLoaded = true;
	}

	private void ScheduleBookmarksSave() {
		if (!bookmarksLoaded || ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		ModuleBookmarksState state = new() {
			Module = module,
			Roots = BookmarkRoots.Select(r => r.ToState()).ToList(),
		};

		List<ModuleBookmarksState> all = appProfileService.Model.ModuleBookmarks
			.Where(m => !string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase))
			.ToList();
		all.Add(state);
		appProfileService.Model.ModuleBookmarks = all;
		appProfileService.ScheduleSave();
	}

}

public record ModuleNavigationActions(Action ShowE621, Action ShowE6AI, Action ShowE926);
