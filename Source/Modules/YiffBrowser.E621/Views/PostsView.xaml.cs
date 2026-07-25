using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using HandyControl.Data;
using RW.Base.WPF.Events;
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
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.BaseFramework.ViewModelServices;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

internal partial class PostsView : UserControl, IDisposable {
	private readonly PostsViewModel viewModel;

	public PostsView(PostTabItem postTabItem) {
		InitializeComponent();

		viewModel = IoC.Resolve<PostsViewModel>()!;
		viewModel.Initialize(postTabItem);
		DataContext = viewModel;
	}

	public void Dispose() {
		viewModel.Dispose();
	}

	private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
		//e.Handled = true;
	}

	private void TextBlock_DragLeave(object sender, DragEventArgs e) {

	}
}

internal class PostsViewModel(
	IViewConfigService viewConfigService,
	IEventAggregator eventAggregator,
	IAppSettingsService appSettingsService,
	IDownloadService downloadService
) : ViewModelBase, IDisposable {
	public IUIObjectService<PostCardListBox> PostsListBoxService => GetService<ITypedUIObjectService>(nameof(PostsListBoxService)).As<PostCardListBox>();
	public IUIObjectService<ButtonPopup> PaginationButtonPopupService => GetService<ITypedUIObjectService>(nameof(PaginationButtonPopupService)).As<ButtonPopup>();


	public IUIObjectService<Grid> DownloadDialogRootService => GetService<ITypedUIObjectService>(nameof(DownloadDialogRootService)).As<Grid>();


	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	public IDialogServiceEx AppSettingsDialog => GetService<IDialogServiceEx>(nameof(AppSettingsDialog));


	public IViewConfigService ViewConfigService { get; } = viewConfigService;
	public IDownloadService DownloadService { get; } = downloadService;

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

	public DownloadMode DownloadMode {
		get => GetProperty(() => DownloadMode);
		set => SetProperty(() => DownloadMode, value);
	}

	public DownloadConfigViewModel? DownloadConfig {
		get => GetProperty(() => DownloadConfig);
		set {
			DownloadConfig?.Dispose();
			SetProperty(() => DownloadConfig, value);
			value?.Initialize();
			if (value != null) {
				DownloadDialogRootService.Object.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
			}
		}
	}

	private CancellationTokenSource? paginationLoadingCts;

	protected override void OnInitializeInRuntime() {
		base.OnInitializeInRuntime();

		MaxPage = 750;

		SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

		eventAggregator.GetEvent<AppSettingsChangedEvent>().Subscribe(OnAppSettingsChanged);
	}

	public void Initialize(PostTabItem postTabItem) {
		TabItem = postTabItem;

		ModuleType = postTabItem.ViewParameter.ModuleType;

		CurrentPage = 1;

		Refresh();
	}

	public void Dispose() {
		eventAggregator.GetEvent<AppSettingsChangedEvent>().Unsubscribe(OnAppSettingsChanged);
	}

	private void OnAppSettingsChanged(AppSettingsChangedEventArgs args) {

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

		TabItem.LoadingStatus.Initialize();
		IsMultiSelecting = false;

		try {
			foreach (PostCardControl item in Items) {
				item.Dispose();
			}
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
				Items.Add(new PostCardControl(this, ViewConfigService, post));
				TabItem.Posts.Add(post);
			}

			PostsListBoxService.Object.Items.Refresh();

			TabItem.LoadingStatus.Done();
		} catch (Exception ex) {
			TabItem.LoadingStatus.Error(ex.Message);
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
			// 如果 MenuDropAlignment 为 true，意味着菜单是“右对齐”的（即向左弹出）
			bool isRightAligned = SystemParameters.MenuDropAlignment;

			popup.HorizontalOffset = 0;

			if (isRightAligned) {
				// 右手习惯：菜单出现在鼠标/手指的左侧
				popup.HorizontalOffset = child.ActualWidth * 0.5;
			} else {
				// 左手习惯：菜单出现在鼠标/手指的右侧
				popup.HorizontalOffset = -child.ActualWidth * 0.5;
			}
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



	private DelegateCommand? downloadSelectedCommand;
	public IDelegateCommand DownloadSelectedCommand => downloadSelectedCommand ??= new(DownloadSelected, CanDownloadSelected);
	private void DownloadSelected() {
		if (CanDownloadSelected()) {
			DownloadMode = DownloadMode.DownloadSelected;
			DownloadConfig = new DownloadSelectedViewModel() {
				ParentViewModel = this,
				Tags = TabItem.Tags,
				AppSettingsDownloadFolder = appSettingsService.Model.DownloadFolderPath,
				Posts = [.. SelectedItems.Select(x => x.Post)],
			};
		}
	}
	private bool CanDownloadSelected() => SelectedItems.IsNotEmpty();



	private DelegateCommand? downloadCurrentPageCommand;
	public IDelegateCommand DownloadCurrentPageCommand => downloadCurrentPageCommand ??= new(DownloadCurrentPage, CanDownloadCurrentPage);
	private void DownloadCurrentPage() {
		if (CanDownloadCurrentPage()) {
			DownloadMode = DownloadMode.DownloadCurrentPage;
			DownloadConfig = new DownloadSelectedViewModel() {
				ParentViewModel = this,
				Tags = TabItem.Tags,
				AppSettingsDownloadFolder = appSettingsService.Model.DownloadFolderPath,
				Posts = [.. Items.Select(x => x.Post)],
			};
		}
	}
	private bool CanDownloadCurrentPage() => Items.IsNotEmpty();



	private DelegateCommand? customDownloadCommand;
	public IDelegateCommand CustomDownloadCommand => customDownloadCommand ??= new(CustomDownload, CanCustomDownload);
	private void CustomDownload() {
		if (CanCustomDownload()) {
			DownloadMode = DownloadMode.CustomDownload;
			DownloadConfig = new CustomDownloadViewModel(TabItem.Api, pageLimit: 75) {
				ParentViewModel = this,
				Tags = TabItem.Tags,
				AppSettingsDownloadFolder = appSettingsService.Model.DownloadFolderPath,
				CurrentPage = CurrentPage,
				MaxPage = MaxPage,
			};
		}
	}
	private bool CanCustomDownload() => true;


	private DelegateCommand? dismissDownloadCommand;
	public IDelegateCommand DismissDownloadCommand => dismissDownloadCommand ??= new(DismissDownload);
	public void DismissDownload() {
		DownloadMode = DownloadMode.None;
	}


	private DelegateCommand? openSettingsCommand;
	public IDelegateCommand OpenSettingsCommand => openSettingsCommand ??= new(OpenSettings);
	private void OpenSettings() {
		AppSettingsDialog.ShowOKCancel(this, null);
	}

}

internal abstract class DownloadConfigViewModel : BindableBase, IDisposable {
	private readonly IE621ProfileService profileService = IoC.GetService<IE621ProfileService>();

	public required PostsViewModel ParentViewModel { get; init; }

	public LoadingStatus LoadingStatus_GetPosts { get; } = new();


	private CancellationTokenSource? cts;

	public required string[] Tags { get; init; }

	public required string AppSettingsDownloadFolder {
		get => GetProperty(() => AppSettingsDownloadFolder);
		set => SetProperty(() => AppSettingsDownloadFolder, value);
	}

	public bool UseCustomPath {
		get => GetProperty(() => UseCustomPath);
		set {
			SetProperty(() => UseCustomPath, value);
			RaiseDestinationFolder();
		}
	}

	public string CustomPath {
		get => GetProperty(() => CustomPath);
		set {
			SetProperty(() => CustomPath, value);
			RaiseDestinationFolder();

			profileService.Model.LastCustomDownloadPath = value;
			profileService.SaveSettings();
		}
	}

	public string DestinationFolder => UseCustomPath ? CustomPath : AppSettingsDownloadFolder;

	public void RaiseDestinationFolder() => RaisePropertyChanged(() => DestinationFolder);
	public DownloadConfigViewModel() {
		UseCustomPath = false;

		CustomPath = profileService.Model.LastCustomDownloadPath.SafeString();
	}


	private class E621PostDownloadable(E621Post post) : IDownloadable {
		public string DownloadUrl => post.File!.URL!;
		public string TargetFileName => $"{post.ID}.{post.File!.Ext}";
		public string? PreviewUrl => post.Preview?.URL;
		public bool CanDownload => post.File != null && post.File.URL.IsNotBlank();
	}


	private AsyncCommand? confirmCommand;
	public IDelegateCommand ConfirmCommand => confirmCommand ??= new(Confirm, CanConfirm);
	protected async Task Confirm() {
		try {
			cts = new CancellationTokenSource();
			CancellationToken token = cts.Token;

			LoadingStatus_GetPosts.Initialize("Loading Posts");

			//await Task.Delay(4000, token);
			IEnumerable<E621Post> posts = await GetPostsAsync(token);
			IEnumerable<E621PostDownloadable> postsDownloadable = posts.Select(x => new E621PostDownloadable(x)).Where(x => x.CanDownload);

			ParentViewModel.DownloadService.StartDownloads(postsDownloadable, DestinationFolder);

			ParentViewModel.DismissDownload();

			LoadingStatus_GetPosts.Done();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			LoadingStatus_GetPosts.Error(ex.Message);
		}
	}

	protected virtual bool CanConfirm() {
		return DestinationFolder.IsNotBlank();
	}



	public IDelegateCommand CancelLoadingCommand => field ??= new DelegateCommand(CancelLoading, CanCancelLoading);
	private void CancelLoading() {
		if (CanCancelLoading()) {
			cts?.Cancel();
			cts?.Dispose();
			cts = null;
		}
	}
	private bool CanCancelLoading() => true;


	public virtual void Dispose() {

	}

	public virtual void Initialize() {

	}


	public abstract Task<IEnumerable<E621Post>> GetPostsAsync(CancellationToken token);
}

internal class DownloadSelectedViewModel : DownloadConfigViewModel {
	public required IReadOnlyList<E621Post> Posts { get; init; }

	public override async Task<IEnumerable<E621Post>> GetPostsAsync(CancellationToken token) {
		return Posts;
	}

	protected override bool CanConfirm() {
		return base.CanConfirm() && Posts.IsNotEmpty();
	}
}

internal class DownloadCurrentPageViewModel : DownloadConfigViewModel {
	public required IReadOnlyList<E621Post> Posts { get; init; }

	public override async Task<IEnumerable<E621Post>> GetPostsAsync(CancellationToken token) {
		return Posts;
	}


	protected override bool CanConfirm() {
		return base.CanConfirm() && Posts.IsNotEmpty();
	}
}

internal class CustomDownloadViewModel(E621API api, int pageLimit) : DownloadConfigViewModel {

	public required int CurrentPage { get; init; }

	public int FromPage {
		get => GetProperty(() => FromPage);
		set => SetProperty(() => FromPage, value);
	}

	public int ToPage {
		get => GetProperty(() => ToPage);
		set => SetProperty(() => ToPage, value);
	}

	public int MaxPage {
		get => GetProperty(() => MaxPage);
		set => SetProperty(() => MaxPage, value);
	}

	public LoadingStatus LoadingStatus { get; } = new();

	private readonly CancellationTokenSource cts = new();
	private AsyncCommand? refreshMaxPageCommand;

	public override void Initialize() {
		base.Initialize();

		FromPage = 1;
		ToPage = 1;

		_ = RefreshMaxPage();
	}

	public IDelegateCommand RefreshMaxPageCommand => refreshMaxPageCommand ??= new(RefreshMaxPage);
	private async Task RefreshMaxPage() {
		try {
			CancellationToken token = cts.Token;
			LoadingStatus.Initialize();

			E621Paginator? r = await api.GetPaginatorAsync(Tags, pageLimit, CurrentPage, token);
			if (r != null && !token.IsCancellationRequested) {
				MaxPage = r.MaxPage;
			} else {
				MaxPage = 0;
			}

			LoadingStatus.Done();
		} catch (OperationCanceledException) {

		} catch (Exception ex) {
			Debug.WriteLine(ex);
			LoadingStatus.ErrorClose(ex.Message);
		}
	}


	private DelegateCommand? selectCurrentPageCommand;
	public IDelegateCommand SelectCurrentPageCommand => selectCurrentPageCommand ??= new(SelectCurrentPage);
	private void SelectCurrentPage() {
		FromPage = CurrentPage;
		ToPage = CurrentPage;
	}


	private DelegateCommand? selectAllPageCommand;
	public IDelegateCommand SelectAllPageCommand => selectAllPageCommand ??= new(SelectAllPage);
	private void SelectAllPage() {
		FromPage = 1;
		ToPage = MaxPage;
	}

	public override async Task<IEnumerable<E621Post>> GetPostsAsync(CancellationToken token) {

		List<E621Post> posts = [];

		int index = 0;
		int count = ToPage - FromPage + 1;
		for (int i = FromPage; i <= ToPage; i++) {
			LoadingStatus_GetPosts.DownloadInfo = $"Loading Posts: Page {i} ({++index}/{count})";
			E621Post[] r = await api.GetPostsByTagsAsync(new E621PostParameters() {
				Tags = Tags,
				PageLimit = pageLimit,
				Page = i,
			}, token);
			posts.AddRange(r);
		}

		return posts;
	}

	protected override bool CanConfirm() {
		return base.CanConfirm() && FromPage <= ToPage;
	}

	public override void Dispose() {
		base.Dispose();

		cts.Cancel();
	}

}