using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using BaseFramework.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using XamlAnimatedGif;
using YB.E621.Models.E621;
using YB.E621.Services;
using YB.E621.Views.Subs;

namespace YB.E621.Views {
	public partial class E621MainWindow : WindowBase {

		public E621MainWindow() {
			InitializeComponent();
			//Test();
		}

		//private async void Test() {
		//	string uri = @"https://static1.e621.net/data/91/3d/913d9dd37fa6d5a3cef1e8e91ef79873.gif";
		//	HttpClient client = new();

		//	try {
		//		HttpRequestMessage request = new(HttpMethod.Get, uri);
		//		HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		//		response.EnsureSuccessStatusCode();
		//		long? contentLength = response.Content.Headers.ContentLength;

		//		MemoryStream memoryStream = new();

		//		using Stream contentStream = await response.Content.ReadAsStreamAsync();

		//		long totalRead = 0L;
		//		byte[] buffer = new byte[8192 * 10];
		//		bool isMoreToRead = true;

		//		do {
		//			int read = await contentStream.ReadAsync(buffer);
		//			if (read == 0) {
		//				isMoreToRead = false;
		//			} else {
		//				await memoryStream.WriteAsync(buffer.AsMemory(0, read));
		//				totalRead += read;

		//				if (contentLength.HasValue) {
		//					double progress = Math.Round((double)totalRead / contentLength.Value * 100, 2);
		//					Debug.WriteLine($"Progress: {progress}%");
		//				}
		//			}
		//		} while (isMoreToRead);

		//		memoryStream.Position = 0; // Reset stream position before usage

		//		Dispatcher.Invoke(() => {
		//			AnimationBehavior.SetAutoStart(MainImage, true);
		//			AnimationBehavior.SetSourceStream(MainImage, memoryStream);
		//		});

		//	} catch (Exception ex) {
		//		Debug.WriteLine(ex.Message);
		//		Dispatcher.Invoke(() => {
		//			MessageBox.Show("Unable to download or display the GIF", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		//		});
		//	}
		//}

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

		private void SettingsButton_Click(object sender, RoutedEventArgs e) {
			AppSettingsDialogViewModel.ShowDialog(GetWindow((DependencyObject)sender));
		}

	}

	public class E621MainWindowViewModel : WindowViewModel<E621MainWindow> {
		private bool isLoggedIn = false;
		private int tabSelectedIndex = 0;

		public ModuleType ModuleType { get; }
		public ModuleNavigationActions ModuleNavigationActions { get; }
		public ObservableCollection<PostsViewModel> Tabs { get; } = [];

		public E621UserService UserService { get; }

		public SearchViewModel SearchViewModel { get; }
		public UserLoginViewModel UserLoginViewModel { get; }
		public UserViewModel UserViewModel { get; }

		public int TabSelectedIndex {
			get => tabSelectedIndex;
			set => SetProperty(ref tabSelectedIndex, value);
		}

		public bool IsLoggedIn {
			get => isLoggedIn;
			set => SetProperty(ref isLoggedIn, value);
		}

		public E621MainWindowViewModel(ModuleType moduleType, ModuleNavigationActions moduleNavigationActions) {
			UserService = E621UserService.GetUserService(moduleType);
			UserService.LoginChanged += UserService_LoginChanged;

			SearchViewModel = new SearchViewModel(moduleType);
			SearchViewModel.SearchSubmit += SearchViewModel_SearchSubmit;

			UserLoginViewModel = new UserLoginViewModel(moduleType);
			UserViewModel = new UserViewModel(moduleType);

			ModuleType = moduleType;
			ModuleNavigationActions = moduleNavigationActions;
			View.Title = $"Yiff Browser - {moduleType}";

			Tabs.Add(new PostsViewModel(ModuleType, ["order:rank"]));
			//Tabs.Add(new PostsViewModel(ModuleType, ["type:gif"]));
			//Tabs.Add(new PostsViewModel(ModuleType, ["type:webm"]));
			TabSelectedIndex = 0;
		}

		protected override async void LoadedOnce(IViewBase viewBase) {
			base.LoadedOnce(viewBase);
			await UserService.Initialize();
		}

		private void SearchViewModel_SearchSubmit(SearchViewModel sender, string[] args) {
			View.SearchPopup.Hide();

			PostsViewModel viewModel = new(ModuleType, args);
			Tabs.Add(viewModel);
			TabSelectedIndex = Tabs.Count - 1;

			viewModel.View.Dispatcher.BeginInvoke(viewModel.View.Focus, DispatcherPriority.Loaded);
		}

		private void UserService_LoginChanged(E621User? sender, E621Post? args) {
			IsLoggedIn = sender != null;
		}

		public ICommand CloseTabCommand => new DelegateCommand<PostsViewModel>(CloseTab);

		private void CloseTab(PostsViewModel model) {
			Tabs.Remove(model);
		}

		public ICommand ShowE621Command => new DelegateCommand(() => {
			View.SitePopup.Hide();
			ModuleNavigationActions.ShowE621();
		});

		public ICommand ShowE6AICommand => new DelegateCommand(() => {
			View.SitePopup.Hide();
			ModuleNavigationActions.ShowE6AI();
		});

		public ICommand ShowE926Command => new DelegateCommand(() => {
			View.SitePopup.Hide();
			ModuleNavigationActions.ShowE926();
		});

	}

	public record ModuleNavigationActions(Action ShowE621, Action ShowE6AI, Action ShowE926);

}
