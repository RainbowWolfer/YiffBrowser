using DevExpress.Mvvm;
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
	IDownloadService downloadService
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
		set => SetProperty(() => TabSelectedIndex, value);
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
	}

	private void Tabs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		BackToTabsCommand.RaiseCanExecuteChanged();
		RaisePropertyChanged(() => Tabs);
	}

	private PostTabItem CreateTabItem(string[] tags) {
		PostTabItem item = new(this, tags);
		return item;
	}


	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		DispatcherService.Dispatcher.BeginInvoke(async () => {
			Initialize();

			//for (int i = 0; i < 20; i++) {
			//	Tabs.Add(CreateTabItem(["ssssssss"]));
			//	//await Task.Delay(10);
			//}

			//Tabs.Add(CreateTabItem(["type:gif", "order:filesize"]));
			//Tabs.Add(CreateTabItem(["type:gif", "order:filesize"]));
			//Tabs.Add(CreateTabItem(["type:gif"]));
			//Tabs.Add(CreateTabItem(["feet"]));
			//Tabs.Add(CreateTabItem(["type:gif"]));
			//Tabs.Add(CreateTabItem(["type:webm"]));
			Tabs.Add(CreateTabItem(["wallpaper", "rating:safe"]));
			//Tabs.Add(CreateTabItem(["order:rank"]));
			TabSelectedIndex = 0;

		}, DispatcherPriority.Background);

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
	}


	private DelegateCommand? backToTabsCommand;
	public IDelegateCommand BackToTabsCommand => backToTabsCommand ??= new(BackToTabs, CanBackToTabs);
	private void BackToTabs() {
		if (CanBackToTabs()) {
			ShowTabsManage = false;
		}
	}
	private bool CanBackToTabs() => Tabs.IsNotEmpty();



	private DelegateCommand? openDownloadCommand;
	public IDelegateCommand OpenDownloadCommand => openDownloadCommand ??= new(OpenDownload);
	private void OpenDownload() {
		DownloadDialog.ShowDialog(this, null);
	}


	private DelegateCommand<PostTabItem>? cloneTabCommand;
	public IDelegateCommand CloneTabCommand => cloneTabCommand ??= new(CloneTab, CanCloneTab);
	private void CloneTab(PostTabItem item) {
		if (CanCloneTab(item)) {

		}
	}
	private bool CanCloneTab(PostTabItem item) => true;


	private DelegateCommand<PostTabItem>? refreshTabCommand;
	public IDelegateCommand RefreshTabCommand => refreshTabCommand ??= new(RefreshTab, CanRefreshTab);
	private void RefreshTab(PostTabItem item) {
		if (CanRefreshTab(item)) {

		}
	}
	private bool CanRefreshTab(PostTabItem item) => true;



	private DelegateCommand<PostTabItem>? appendSearchCommand;
	public IDelegateCommand AppendSearchCommand => appendSearchCommand ??= new(AppendSearch, CanAppendSearch);
	private void AppendSearch(PostTabItem item) {
		if (CanAppendSearch(item)) {

		}
	}
	private bool CanAppendSearch(PostTabItem item) => true;

	public ICommand CloseTabCommand => new DelegateCommand<PostTabItem>(CloseTab, CanCloseTab);
	private void CloseTab(PostTabItem item) {
		if (CanCloseTab(item)) {
			item.Dispose();
			Tabs.Remove(item);
		}
	}
	private bool CanCloseTab(PostTabItem item) => item != null;


	private DelegateCommand<PostTabItem>? closeAllTabsCommand;
	public IDelegateCommand CloseAllTabsCommand => closeAllTabsCommand ??= new(CloseAllTabs, CanCloseAllTabs);
	private void CloseAllTabs(PostTabItem item) {
		if (CanCloseAllTabs(item)) {
			Tabs.ForEach(x => x.Dispose());
			Tabs.Clear();
		}
	}
	private bool CanCloseAllTabs(PostTabItem item) => true;



	private DelegateCommand<PostTabItem>? closeAllBeforeTabsCommand;
	public IDelegateCommand CloseAllBeforeTabsCommand => closeAllBeforeTabsCommand ??= new(CloseAllBeforeTabs, CanCloseAllBeforeTabs);
	private void CloseAllBeforeTabs(PostTabItem item) {
		if (CanCloseAllBeforeTabs(item)) {
			int index = Tabs.IndexOf(item);
			if (index > 0) {
				// 获取当前项之前的所有项
				PostTabItem[] toRemove = [.. Tabs.Take(index)];
				foreach (PostTabItem tab in toRemove) {
					tab.Dispose();
					Tabs.Remove(tab);
				}
			}
		}
	}
	private bool CanCloseAllBeforeTabs(PostTabItem item) => item != null && Tabs.IndexOf(item) > 0;



	private DelegateCommand<PostTabItem>? closeAllAfterTabsCommand;
	public IDelegateCommand CloseAllAfterTabsCommand => closeAllAfterTabsCommand ??= new(CloseAllAfterTabs, CanCloseAllAfterTabs);
	private void CloseAllAfterTabs(PostTabItem item) {
		if (CanCloseAllAfterTabs(item)) {
			int index = Tabs.IndexOf(item);
			if (index >= 0 && index < Tabs.Count - 1) {
				// 获取当前项之后的所有项
				PostTabItem[] toRemove = [.. Tabs.Skip(index + 1)];
				foreach (PostTabItem tab in toRemove) {
					tab.Dispose();
					Tabs.Remove(tab);
				}
			}
		}
	}
	private bool CanCloseAllAfterTabs(PostTabItem item) => item != null && Tabs.IndexOf(item) >= 0 && Tabs.IndexOf(item) < Tabs.Count - 1;



	private DelegateCommand<PostTabItem>? closeAllOtherTabsCommand;
	public IDelegateCommand CloseAllOtherTabsCommand => closeAllOtherTabsCommand ??= new(CloseAllOtherTabs, CanCloseAllOtherTabs);
	private void CloseAllOtherTabs(PostTabItem item) {
		if (CanCloseAllOtherTabs(item)) {
			// 找出所有不等于当前选中项的标签页
			PostTabItem[] toRemove = [.. Tabs.Where(x => x != item)];
			foreach (PostTabItem tab in toRemove) {
				tab.Dispose();
				Tabs.Remove(tab);
			}
		}
	}
	private bool CanCloseAllOtherTabs(PostTabItem item) => item != null && Tabs.Count > 1 && Tabs.Contains(item);


}

public record ModuleNavigationActions(Action ShowE621, Action ShowE6AI, Action ShowE926);
