using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

	public class PostsViewModel : UserControlViewModel<PostsView> {
		public const double ItemWidth = 396;
		public const double ItemHeight = 50;

		private bool isLoading = false;
		private int currentPage = 1;
		private bool isMultiSelecting = false;
		private string multiSelectingText = string.Empty;
		private E621Post? currentPost = null;

		public ObservableCollection<PostCardControl> Items { get; } = [];
		public ObservableCollection<PostCardControl> SelectedItems { get; } = [];

		public ModuleType SiteType { get; }
		public string[] Tags { get; }

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public int CurrentPage {
			get => currentPage;
			set {
				SetProperty(ref currentPage, Math.Clamp(value, 1, int.MaxValue));
				RaisePropertyChanged(nameof(CanGoLeft));
			}
		}

		public bool CanGoLeft => CurrentPage > 1;

		public bool IsMultiSelecting {
			get => isMultiSelecting;
			set {
				SetProperty(ref isMultiSelecting, value);
				foreach (PostCardControl item in Items) {
					item.IsSelected = false;
				}
			}
		}

		public string MultiSelectingText {
			get => multiSelectingText;
			set => SetProperty(ref multiSelectingText, value);
		}

		private (double left, double right) LastSiedWidth { get; set; } = (0, 1);

		public E621Post? CurrentPost {
			get => currentPost;
			set {
				SetProperty(ref currentPost, value);
				if (value is null) {
					LastSiedWidth = (View.LeftColumnDef.Width.Value, View.RightColumnDef.Width.Value);
					View.LeftColumnDef.Width = new GridLength(1, GridUnitType.Star);
					View.RightColumnDef.Width = new GridLength(0, GridUnitType.Pixel);
					View.RightColumnDef.MinWidth = 0;
				} else {
					View.LeftColumnDef.Width = new GridLength(LastSiedWidth.left, GridUnitType.Star);
					View.RightColumnDef.Width = new GridLength(LastSiedWidth.right, GridUnitType.Star);
					View.RightColumnDef.MinWidth = 150;
				}
			}
		}

		public PostsViewModel(ModuleType siteType, string[] tags) {
			SiteType = siteType;
			Tags = tags;

			SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

			CurrentPost = null;
			LastSiedWidth = (0, 1);
		}

		private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			if (IsMultiSelecting) {
				MultiSelectingText = $"{Items.Where(x => x.IsSelected).Count()}/{Items.Count}";
			} else {
				MultiSelectingText = string.Empty;
			}
		}

		protected override void LoadedOnce(IViewBase viewBase) {
			base.LoadedOnce(viewBase);
			Refrsh();
		}

		public ICommand PreviousPageCommand => new DelegateCommand(PreviousPage);
		public ICommand NextPageCommand => new DelegateCommand(NextPage);

		private void PreviousPage() {
			CurrentPage -= 1;
			Refrsh();
		}

		private void NextPage() {
			CurrentPage += 1;
			Refrsh();
		}

		public ICommand DownloadCommand => new DelegateCommand(Download);

		private void Download() {

		}

		public ICommand RefreshCommand => new DelegateCommand(Refrsh);

		private async void Refrsh() {
			if (IsLoading) {
				return;
			}

			IsLoading = true;
			IsMultiSelecting = false;

			Items.Clear();
			E621Post[] posts = await E621API.GetPostsByTagsAsync(new E621PostParameters() {
				Tags = Tags,
				Page = CurrentPage,
			});

			foreach (E621Post item in posts) {
				if (item.HasNoValidURLs()) {
					continue;
				}
				Items.Add(new PostCardControl(item));
			}

			IsLoading = false;
		}

		public ICommand ViewPostDetailCommand => new DelegateCommand<E621Post?>(ViewPostDetail);

		private void ViewPostDetail(E621Post? post) {
			CurrentPost = post;
			//SelectedItems.FirstOrDefault()?.BringIntoView();
		}

		public ICommand QuitPostDetailViewCommand => new DelegateCommand(QuitPostDetailView);

		private void QuitPostDetailView() {
			CurrentPost = null;
			//SelectedItems.FirstOrDefault()?.BringIntoView();
		}

	}
}
