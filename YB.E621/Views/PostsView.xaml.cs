using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using YB.E621.Controls;
using YB.E621.Models;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views {
	public partial class PostsView : UserControlBase {
		public PostsView() {
			InitializeComponent();
		}

		private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
			//e.Handled = true;
		}

		private void TextBlock_DragLeave(object sender, DragEventArgs e) {

		}
	}

	public class PostsViewModel(ModuleType siteType, string[] tags) : UserControlViewModel<PostsView> {
		public const double ItemWidth = 396;
		public const double ItemHeight = 50;

		private bool isLoading = false;

		public ObservableCollection<PostCardControl> Items { get; } = [];
		public ModuleType SiteType { get; } = siteType;
		public string[] Tags { get; } = tags;

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		protected override void LoadedOnce(IViewBase viewBase) {
			base.LoadedOnce(viewBase);
			Refrsh();
		}

		public ICommand RefreshCommand => new DelegateCommand(Refrsh);

		private async void Refrsh() {
			if (IsLoading) {
				return;
			}

			IsLoading = true;

			Items.Clear();
			E621Post[] posts = await E621API.GetPostsByTagsAsync(new E621PostParameters() {
				Tags = Tags,
			});

			foreach (E621Post item in posts) {
				if (item.HasNoValidURLs()) {
					continue;
				}
				Items.Add(new PostCardControl(item));
			}

			IsLoading = false;
		}
	}
}
