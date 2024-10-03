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
			e.Handled = true;
		}
	}

	public class PostsViewModel(ModuleType siteType) : UserControlViewModel<PostsView> {
		public const double ItemWidth = 380;
		public const double ItemHeight = 50;

		public ObservableCollection<PostCardControl> Items { get; } = [];
		public ModuleType SiteType { get; } = siteType;


		protected override void Loaded(IViewBase viewBase) {
			base.Loaded(viewBase);
			Refrsh();
		}

		public ICommand RefreshCommand => new DelegateCommand(Refrsh);

		private async void Refrsh() {
			Items.Clear();
			E621Post[] posts = await E621API.GetPostsByTagsAsync(new E621PostParameters() {
				Tags = ["belly_riding"],
			});

			foreach (E621Post item in posts) {
				if (item.HasNoValidURLs()) {
					continue;
				}
				Items.Add(new PostCardControl(item));
			}

		}
	}
}
