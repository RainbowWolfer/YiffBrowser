using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using HandyControl.Data;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.ViewModelServices;
using RW.Common;
using RW.Common.Helpers;
using RW.Common.WPF.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Enums;
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
	public IUIObjectService<PostCardListBox> PostsListBoxService => GetService<ITypedUIObjectService>(nameof(PostsListBoxService)).As<PostCardListBox>();
	public IUIObjectService<ButtonPopup> PaginationButtonPopupService => GetService<ITypedUIObjectService>(nameof(PaginationButtonPopupService)).As<ButtonPopup>();

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();


	public const double ItemWidth = 396;
	public const double ItemHeight = 50;

	public event TypedEventHandler<PostsViewModel, E621Post?>? CurrentPostChanged;

	public ObservableCollection<PostCardControl> Items { get; } = [];
	public ObservableCollection<PostCardControl> SelectedItems { get; } = [];

	public PostTabItem TabItem {
		get => GetProperty(() => TabItem);
		private set => SetProperty(() => TabItem, value);
	}

	public bool IsLoadingPagination {
		get => GetProperty(() => IsLoadingPagination);
		set => SetProperty(() => IsLoadingPagination, value);
	}

	public int MaxPage {
		get => GetProperty(() => MaxPage);
		set => SetProperty(() => MaxPage, value);
	}

	public int CurrentPage {
		get => GetProperty(() => CurrentPage);
		set {
			SetProperty(() => CurrentPage, NumberHelper.Clamp(value, 1, int.MaxValue));
			RaisePropertyChanged(() => CanGoLeft);
		}
	}

	public bool CanGoLeft => CurrentPage > 1;

	public int PaginationInputPage {
		get => GetProperty(() => PaginationInputPage);
		set => SetProperty(() => PaginationInputPage, value);
	}

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

	public E621Post? LastViewedPost {
		get => GetProperty(() => LastViewedPost);
		set => SetProperty(() => LastViewedPost, value);
	}

	public bool CurrentHasPost => CurrentPost != null;


	private CancellationTokenSource? paginationLoadingCts;

	protected override void OnInitializeInRuntime() {
		base.OnInitializeInRuntime();

		MaxPage = 750;

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
		if (TabItem.LoadingStatus.ShowLoading) {
			return;
		}

		TabItem.LoadingStatus.InitialLoading();
		IsMultiSelecting = false;

		try {
			Items.Clear();
			TabItem.Posts.Clear();

			paginationLoadingCts?.Cancel();
			paginationLoadingCts = new CancellationTokenSource();

			int pageLimit = 75;

			_ = Task.Run(async () => {
				CancellationToken token = paginationLoadingCts.Token;
				IsLoadingPagination = true;
				try {
					E621Paginator? r = await TabItem.Api.GetPaginatorAsync(TabItem.Tags, pageLimit, CurrentPage, token);
					if (r != null && !token.IsCancellationRequested) {
						DispatcherService.Invoke(() => {
							MaxPage = r.MaxPage;
						});
					}
				} catch (OperationCanceledException) {

				} catch (Exception ex) {
					Debug.WriteLine(ex);
				} finally {
					if (!token.IsCancellationRequested) {
						IsLoadingPagination = false;
					}
				}
			});

			E621Post[] posts = await TabItem.Api.GetPostsByTagsAsync(new E621PostParameters() {
				Tags = TabItem.Tags,
				Page = CurrentPage,
				PageLimit = pageLimit,
			});

			foreach (E621Post post in posts) {
				if (post.HasNoValidURLs()) {
					continue;
				}
				Items.Add(new PostCardControl(post));
				TabItem.Posts.Add(post);
			}

			TabItem.LoadingStatus.DoneLoading();
		} catch (Exception ex) {
			TabItem.LoadingStatus.LoadingError(ex.Message);
		} finally {

		}


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
		LastViewedPost = post;
		CurrentPost = post;
	}

	public ICommand QuitPostDetailViewCommand => new DelegateCommand(QuitPostDetailView);
	public void QuitPostDetailView() {
		if (CurrentPost != null) {
			int index = TabItem.Posts.IndexOf(x => x.ID == CurrentPost.ID);
			PostCardControl? item = Items.ElementAtOrDefault(index);
			if (item != null) {
				PostsListBoxService.Object.ScrollIntoView(item);
			}
		}
		CurrentPost = null;
	}


	private DelegateCommand? nextPostCommand;
	public IDelegateCommand NextPostCommand => nextPostCommand ??= new(NextPost, CanNextPost);
	public void NextPost() {
		if (CanNextPost()) {
			int index = TabItem.Posts.IndexOf(x => x.ID == CurrentPost.ID);
			int targetIndex = index + 1;

			if (targetIndex >= TabItem.Posts.Count) {
				targetIndex = 0;
			}

			E621Post post = TabItem.Posts[targetIndex];
			ViewPostDetailDirect(post);
		}
	}
	[MemberNotNullWhen(true, nameof(CurrentPost))]
	private bool CanNextPost() {
		return CurrentPost != null;
	}


	private DelegateCommand? previousPostCommand;
	public IDelegateCommand PreviousPostCommand => previousPostCommand ??= new(PreviousPost, CanPreviousPost);
	public void PreviousPost() {
		if (CanPreviousPost()) {
			int index = TabItem.Posts.IndexOf(x => x.ID == CurrentPost.ID);
			int targetIndex = index - 1;

			if (targetIndex < 0) {
				targetIndex = TabItem.Posts.Count - 1;
			}

			E621Post post = TabItem.Posts[targetIndex];
			ViewPostDetailDirect(post);
		}
	}
	[MemberNotNullWhen(true, nameof(CurrentPost))]
	private bool CanPreviousPost() {
		return CurrentPost != null;
	}




	private DelegateCommand? locateLastViewedCommand;
	public IDelegateCommand LocateLastViewedCommand => locateLastViewedCommand ??= new(LocateLastViewed, CanLocateLastViewed);
	private void LocateLastViewed() {
		if (CanLocateLastViewed()) {
			if (Items.FirstOrDefault(x => x.Post.ID == LastViewedPost.ID) is { } found) {
				PostsListBoxService.Object.ScrollIntoView(found);
			}
		}
	}
	[MemberNotNullWhen(true, nameof(LastViewedPost))]
	private bool CanLocateLastViewed() => LastViewedPost != null;



	private DelegateCommand? openLastViewedPostCommand;
	public IDelegateCommand OpenLastViewedPostCommand => openLastViewedPostCommand ??= new(OpenLastViewedPost, CanOpenLastViewedPost);
	private void OpenLastViewedPost() {
		if (CanOpenLastViewedPost()) {
			ViewPostDetailDirect(LastViewedPost);
		}
	}
	[MemberNotNullWhen(true, nameof(LastViewedPost))]
	private bool CanOpenLastViewedPost() => LastViewedPost != null;




	private DelegateCommand<FunctionEventArgs<int>>? pageUpdatedCommand;
	public IDelegateCommand PageUpdatedCommand => pageUpdatedCommand ??= new(PageUpdated);
	private void PageUpdated(FunctionEventArgs<int> args) {
		Refresh();
		PaginationButtonPopupService.Object.Hide();
	}


	private DelegateCommand? paginationButtonPopupLoadedCommand;
	public IDelegateCommand PaginationButtonPopupLoadedCommand => paginationButtonPopupLoadedCommand ??= new(PaginationButtonPopupLoaded);
	private void PaginationButtonPopupLoaded() {
		ButtonPopup popup = PaginationButtonPopupService.Object;

		if (popup.Child is FrameworkElement child) {
			popup.HorizontalOffset = (-child.ActualWidth / 2)/* + 100*/;
		}

	}


	private DelegateCommand? jumpCommand;
	public IDelegateCommand JumpCommand => jumpCommand ??= new(Jump, CanJump);
	private void Jump() {
		if (CanJump()) {
			CurrentPage = PaginationInputPage;
			Refresh();
			PaginationButtonPopupService.Object.Hide();
		}
	}
	private bool CanJump() => true;

}
