using BaseFramework;
using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.Services;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using BaseFramework.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;
using YB.E621.Services;
using YB.E621.Views.Subs;

namespace YB.E621.Views {
	public partial class E621MainWindow : WindowBase {

		public E621MainWindow() {
			InitializeComponent();
		}

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
