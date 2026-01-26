using BaseFramework.Services;
using BaseFramework.ViewModelServices;
using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using RW.Common.WPF.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Interfaces;
using YB.E621.Models.E621;
using YB.E621.Parameters;
using YB.E621.Services;
using YB.E621.ViewModels;
using YB.E621.Views.DockPanels;

namespace YB.E621.Views;

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

internal class E621MainViewModel(IApplication application, IAppManager appManager, IThemeManager themeManager) : ViewModelBase {

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControl => GetService<ITypedUIObjectService>(nameof(UserControl)).As<UserControl>();

	public IUIObjectService<ButtonPopup> SearchPopupService => GetService<ITypedUIObjectService>(nameof(SearchPopupService)).As<ButtonPopup>();
	public IUIObjectService<ButtonPopup> SitePopupService => GetService<ITypedUIObjectService>(nameof(SitePopupService)).As<ButtonPopup>();

	public IDialogServiceEx AppSettingsDialog => GetService<IDialogServiceEx>(nameof(AppSettingsDialog));

	public IAppManager AppManager { get; } = appManager;

	public ViewParameter? ViewParameter { get; private set; }

	public ObservableCollection<PostTabItem> Tabs { get; } = [];

	public E621UserService UserService {
		get => GetProperty(() => UserService);
		private set => SetProperty(() => UserService, value);
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

		UserService = E621UserService.GetUserService(parameter.ModuleType);
		UserService.LoginChanged += UserService_LoginChanged;

		DockPanelManager.Initialize(application, this);
	}

	private PostTabItem CreateTabItem(string[] tags) {
		PostTabItem item = new(this, tags);
		return item;
	}


	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		DispatcherService.Dispatcher.Invoke(() => {
			Initialize();

			Tabs.Add(CreateTabItem(["order:rank"]));

			//Tabs.Add(new PostTabItem(ModuleType, ["type:gif", "order:filesize"]));
			//Tabs.Add(new PostTabItem(ModuleType, ["type:gif", "order:filesize"]));
			//Tabs.Add(new PostTabItem(ModuleType, ["type:gif"]));
			//Tabs.Add(new PostTabItem(ModuleType, ["feet"]));
			//Tabs.Add(new PostTabItem(ModuleType, ["type:gif"]));
			//Tabs.Add(new PostTabItem(ModuleType, ["type:webm"]));
			//Tabs.Add(CreateTabItem(["wallpaper", "rating:safe"]));
			TabSelectedIndex = 0;

		}, DispatcherPriority.Loaded);

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

		}

	}

	private void UserService_LoginChanged(E621User? sender, E621Post? args) {
		IsLoggedIn = sender != null;
	}

	public ICommand CloseTabCommand => new DelegateCommand<PostTabItem>(CloseTab);
	private void CloseTab(PostTabItem item) {
		Tabs.Remove(item);
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
	}

	public void ShowSearchHistoryPanel() {
		SelectedDockPanelItem = DockPanelManager.ShowSingle<SearchHistoryPanelItem>();
		SearchPopupService.Object.Hide();
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


}

public record ModuleNavigationActions(Action ShowE621, Action ShowE6AI, Action ShowE926);
