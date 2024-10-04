using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using BaseFramework.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using YB.E621.Models;
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

		public ModuleType SiteType { get; }

		public ObservableCollection<PostsViewModel> Tabs { get; } = [];

		public SearchViewModel SearchViewModel { get; } = new();
		public UserLoginViewModel UserLoginViewModel { get; } = new();
		public UserViewModel UserViewModel { get; } = new();

		public int TabSelectedIndex {
			get => tabSelectedIndex;
			set => SetProperty(ref tabSelectedIndex, value);
		}

		public bool IsLoggedIn {
			get => isLoggedIn;
			set => SetProperty(ref isLoggedIn, value);
		}

		public E621MainWindowViewModel(ModuleType siteType) {
			SearchViewModel.SearchSubmit += SearchViewModel_SearchSubmit;
			E621UserService.LoginChanged += E621UserService_LoginChanged;

			SiteType = siteType;
			View.Title = $"{siteType}";

			Tabs.Add(new PostsViewModel(SiteType, [""]));
			TabSelectedIndex = 0;
		}

		protected override async void LoadedOnce(IViewBase viewBase) {
			base.LoadedOnce(viewBase);
			await E621UserService.Initialize();
		}

		private void SearchViewModel_SearchSubmit(SearchViewModel sender, string[] args) {
			View.SearchPopup.Hide();

			PostsViewModel viewModel = new(SiteType, args);
			Tabs.Add(viewModel);
			TabSelectedIndex = Tabs.Count - 1;

			viewModel.View.Dispatcher.BeginInvoke(viewModel.View.Focus, DispatcherPriority.Loaded);
		}

		private void E621UserService_LoginChanged(E621User? sender, E621Post? args) {
			IsLoggedIn = sender != null;
		}

		public ICommand CloseTabCommand => new DelegateCommand<PostsViewModel>(CloseTab);

		private void CloseTab(PostsViewModel model) {
			Tabs.Remove(model);
		}

	}

}
