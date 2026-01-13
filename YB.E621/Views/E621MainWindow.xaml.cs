using BaseFramework;
using BaseFramework.Interfaces;
using BaseFramework.ViewModelServices;
using BaseFramework.Views;
using DevExpress.Mvvm;
using HandyControl.Themes;
using HandyControl.Tools;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using RW.Common.WPF.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;
using YB.E621.Parameters;
using YB.E621.Services;
using YB.E621.ViewModels;
using YB.E621.Views.Subs;

namespace YB.E621.Views;

public partial class E621MainWindow : WindowBase, IMainWindow {

	public E621MainWindow(ViewParameter viewParameter) {
		InitializeComponent();

		E621MainWindowViewModel viewModel = IoC.Resolve<E621MainWindowViewModel>()!;
		viewModel.Initialize(viewParameter);
		DataContext = viewModel;

		//MeidaElement.LoadedBehavior = MediaState.Play;
		//MeidaElement.Clock.

		Test();
	}

	private async void Test() {

		//MainImage.GifSource = @"https://static1.e621.net/data/91/3d/913d9dd37fa6d5a3cef1e8e91ef79873.gif";
		//MainImage.StartAnimation();

		//string uri = @"https://static1.e621.net/data/91/3d/913d9dd37fa6d5a3cef1e8e91ef79873.gif";
		//HttpClient client = new();

		//try {
		//	HttpRequestMessage request = new(HttpMethod.Get, uri);
		//	HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		//	response.EnsureSuccessStatusCode();
		//	long? contentLength = response.Content.Headers.ContentLength;

		//	MemoryStream memoryStream = new();

		//	using Stream contentStream = await response.Content.ReadAsStreamAsync();

		//	long totalRead = 0L;
		//	byte[] buffer = new byte[8192 * 10];
		//	bool isMoreToRead = true;

		//	do {
		//		int read = await contentStream.ReadAsync(buffer);
		//		if (read == 0) {
		//			isMoreToRead = false;
		//		} else {
		//			await memoryStream.WriteAsync(buffer.AsMemory(0, read));
		//			totalRead += read;

		//			if (contentLength.HasValue) {
		//				double progress = Math.Round((double)totalRead / contentLength.Value * 100, 2);
		//				Debug.WriteLine($"Progress: {progress}%");
		//			}
		//		}
		//	} while (isMoreToRead);

		//	memoryStream.Position = 0; // Reset stream position before usage

		//	GifBitmapDecoder decoder = new(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

		//	Dispatcher.Invoke(() => {

		//	});

		//} catch (Exception ex) {
		//	Debug.WriteLine(ex.Message);
		//	Dispatcher.Invoke(() => {
		//		MessageBox.Show("Unable to download or display the GIF", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		//	});
		//}
	}

	//private async void Test() {

	//	string uri = @"https://static1.e621.net/data/91/3d/913d9dd37fa6d5a3cef1e8e91ef79873.gif";

	//	using HttpClient client = new();
	//	byte[] data = await client.GetByteArrayAsync(uri);

	//	MemoryStream stream = new(data);

	//	Dispatcher.Invoke(() => {
	//		AnimationBehavior.SetAutoStart(MainImage, true);
	//		AnimationBehavior.SetSourceStream(MainImage, stream);
	//	});
	//}

}

public class E621MainWindowViewModel(IAppManager appManager) : ViewModelBase {

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public ICurrentWindowServiceEx CurrentWindowService => GetService<ICurrentWindowServiceEx>();

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

	public void Initialize(ViewParameter parameter) {
		ViewParameter = parameter;

		UserService = E621UserService.GetUserService(parameter.ModuleType);
		UserService.LoginChanged += UserService_LoginChanged;

		//Tabs.Add(new PostTabItem(ModuleType, ["order:rank"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["type:gif", "order:filesize"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["type:gif", "order:filesize"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["type:gif"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["feet"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["type:gif"]));
		//Tabs.Add(new PostTabItem(ModuleType, ["type:webm"]));
		TabSelectedIndex = 0;
	}


	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		if (ViewParameter != null) {
			CurrentWindowService.GetWindow().Title = $"{AppConfig.DisplayAppName} - {ViewParameter.ModuleType}";
		}
		DispatcherService.Dispatcher.Invoke(Initialize, DispatcherPriority.Loaded);
	}


	private async Task Initialize() {
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

	private void SearchViewModel_SearchSubmit(SearchViewModel sender, string[] args) {
		SearchPopupService.Object.Hide();

		if (ViewParameter != null) {

			PostTabItem item = new(ViewParameter.ModuleType, args);
			Tabs.Add(item);
			TabSelectedIndex = Tabs.Count - 1;

			DispatcherService.Dispatcher.BeginInvoke(() => {
				CurrentWindowService.GetWindow().Focus();
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
		if (ThemeManager.Current.ActualApplicationTheme is ApplicationTheme.Dark) {
			ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
		} else {
			ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
		}
	}


}

public record ModuleNavigationActions(Action ShowE621, Action ShowE6AI, Action ShowE926);
