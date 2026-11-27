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
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views.DockPanels;

namespace YiffBrowser.E621.ViewModels;

internal partial class E621MainViewModel(
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
		set {
			if (SetProperty(() => IsDockPanelCollapsed, value)) {
				expandDockPanelCommand?.RaiseCanExecuteChanged();
			}
		}
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

	private PostTabItem CreateTabFromRecord(ClosedTabRecord record) {
		return CreateTabFromKind(record.Kind, record.Tags, record.Page, record.PoolId, record.RelationsRootPostId);
	}

	private PostTabItem CreateTabFromSession(TabSessionState tab) {
		PostTabKind kind = Enum.IsDefined(typeof(PostTabKind), tab.Kind) ? (PostTabKind)tab.Kind : PostTabKind.Search;
		return CreateTabFromKind(kind, tab.Tags, Math.Max(1, tab.CurrentPage), tab.PoolId, tab.RelationsRootPostId);
	}

	private PostTabItem CreateTabFromKind(
		PostTabKind kind,
		string[]? tags,
		int page,
		int? poolId,
		int? relationsRootPostId
	) {
		return kind switch {
			PostTabKind.Pool when poolId is int pid => PostTabItem.CreatePool(this, pid, page),
			PostTabKind.Relations when relationsRootPostId is int rid => PostTabItem.CreateRelations(this, rid),
			_ => CreateTabItem(tags is { Length: > 0 } ? tags : ["wallpaper", "rating:safe"], page),
		};
	}

	private PostTabItem CloneTabItem(PostTabItem item) {
		return item.Kind switch {
			PostTabKind.Pool when item.PoolId is int pid => PostTabItem.CreatePool(this, pid, item.GetContentPage()),
			PostTabKind.Relations when item.RelationsRootPostId is int rid => PostTabItem.CreateRelations(this, rid),
			_ => CreateTabItem(item.Tags, item.GetContentPage()),
		};
	}

	public void OpenPoolTab(int poolId) {
		if (ViewParameter == null || poolId <= 0) {
			return;
		}

		int existing = Tabs.ToList().FindIndex(t => t.Kind == PostTabKind.Pool && t.PoolId == poolId);
		if (existing >= 0) {
			TabSelectedIndex = existing;
			return;
		}

		PostTabItem item = PostTabItem.CreatePool(this, poolId);
		Tabs.Add(item);
		TabSelectedIndex = Tabs.Count - 1;
	}

	public void OpenRelationsTab(int rootPostId) {
		if (ViewParameter == null || rootPostId <= 0) {
			return;
		}

		int existing = Tabs.ToList().FindIndex(t => t.Kind == PostTabKind.Relations && t.RelationsRootPostId == rootPostId);
		if (existing >= 0) {
			TabSelectedIndex = existing;
			return;
		}

		PostTabItem item = PostTabItem.CreateRelations(this, rootPostId);
		Tabs.Add(item);
		TabSelectedIndex = Tabs.Count - 1;
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
				CurrentPage = t.GetContentPage(),
				Kind = (int)t.Kind,
				PoolId = t.PoolId,
				RelationsRootPostId = t.RelationsRootPostId,
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
					Tabs.Add(CreateTabFromSession(tab));
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




	private DelegateCommand? openDownloadCommand;
	public IDelegateCommand OpenDownloadCommand => openDownloadCommand ??= new(OpenDownload);
	private void OpenDownload() {
		DownloadDialog.ShowDialog(this, null);
	}
}
