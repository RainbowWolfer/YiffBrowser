using YiffBrowser.BaseFramework.Enums;
using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Common;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

internal partial class PostsView : UserControl {
	public PostsView(PostTabItem postTabItem) {
		InitializeComponent();

		PostsViewModel viewModel = IoC.Resolve<PostsViewModel>()!;
		viewModel.Initialize(postTabItem);
		DataContext = viewModel;
	}

	private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
		//e.Handled = true;
	}

	private void TextBlock_DragLeave(object sender, DragEventArgs e) {

	}
}

internal class PostsViewModel() : ViewModelBase {
	public const double ItemWidth = 396;
	public const double ItemHeight = 50;

	public event TypedEventHandler<PostsViewModel, E621Post?>? CurrentPostChanged;

	public ObservableCollection<PostCardControl> Items { get; } = [];
	public ObservableCollection<PostCardControl> SelectedItems { get; } = [];

	public PostTabItem TabItem {
		get => GetProperty(() => TabItem);
		private set => SetProperty(() => TabItem, value);
	}

	public bool IsLoading {
		get => GetProperty(() => IsLoading);
		set => SetProperty(() => IsLoading, value);
	}

	public int CurrentPage {
		get => GetProperty(() => CurrentPage);
		set {
			SetProperty(() => CurrentPage, NumberHelper.Clamp(value, 1, int.MaxValue));
			RaisePropertyChanged(() => CanGoLeft);
		}
	}

	public bool CanGoLeft => CurrentPage > 1;

	public bool IsMultiSelecting {
		get => GetProperty(() => IsMultiSelecting);
		set {
			SetProperty(() => IsMultiSelecting, value);
			UpdateMultiSelectingText();
			foreach (PostCardControl item in Items) {
				item.IsSelected = false;
			}
		}
	}

	public string MultiSelectingText {
		get => GetProperty(() => MultiSelectingText);
		set => SetProperty(() => MultiSelectingText, value);
	}

	public ModuleType ModuleType {
		get => GetProperty(() => ModuleType);
		private set => SetProperty(() => ModuleType, value);
	}

	public E621Post? CurrentPost {
		get => GetProperty(() => CurrentPost);
		set {
			SetProperty(() => CurrentPost, value);
			RaisePropertyChanged(() => CurrentHasPost);
			CurrentPostChanged?.Invoke(this, value);
		}
	}

	public bool CurrentHasPost => CurrentPost != null;

	protected override void OnInitializeInRuntime() {
		base.OnInitializeInRuntime();

		SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
	}


	public void Initialize(PostTabItem postTabItem) {
		TabItem = postTabItem;

		ModuleType = postTabItem.ViewParameter.ModuleType;

		CurrentPage = 1;

		Refresh();
	}

	private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		UpdateMultiSelectingText();
	}

	private void UpdateMultiSelectingText() {
		if (IsMultiSelecting) {
			MultiSelectingText = $"{Items.Count(x => x.IsSelected)}/{Items.Count}";
		} else {
			MultiSelectingText = string.Empty;
		}
	}


	//private DelegateCommand? loadedCommand;
	//public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	//private void Loaded() {

	//}

	public ICommand PreviousPageCommand => new DelegateCommand(PreviousPage);
	public ICommand NextPageCommand => new DelegateCommand(NextPage);

	private void PreviousPage() {
		CurrentPage -= 1;
		Refresh();
	}

	private void NextPage() {
		CurrentPage += 1;
		Refresh();
	}

	public ICommand DownloadCommand => new DelegateCommand(Download);

	private void Download() {

	}

	public ICommand RefreshCommand => new DelegateCommand(Refresh);
	private async void Refresh() {
		//return;
		if (IsLoading) {
			return;
		}

		IsLoading = true;
		IsMultiSelecting = false;

		Items.Clear();
		E621Post[] posts = await TabItem.Api.GetPostsByTagsAsync(new E621PostParameters() {
			Tags = TabItem.Tags,
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
	public ICommand ViewPostDetailCommandDirect => new DelegateCommand<E621Post?>(ViewPostDetailDirect);

	private void ViewPostDetail(E621Post? post) {
		if (IsMultiSelecting) {
			return;
		}
		ViewPostDetailDirect(post);
	}

	private void ViewPostDetailDirect(E621Post? post) {
		CurrentPost = post;
	}

	public ICommand QuitPostDetailViewCommand => new DelegateCommand(QuitPostDetailView);
	public void QuitPostDetailView() {
		CurrentPost = null;
	}

}
