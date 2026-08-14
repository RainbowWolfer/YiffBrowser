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
using YiffBrowser.BaseFramework.Utilities;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.BaseFramework.ViewModelServices;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

internal partial class PostsView : UserControl, IPostTabContent {
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

	public int CurrentPage => viewModel.CurrentPage;

	public void RefreshPosts() {
		if (viewModel.RefreshCommand.CanExecute(null)) {
			viewModel.RefreshCommand.Execute(null);
		}
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
	IDownloadService downloadService,
	IDownloadIndexService downloadIndexService
) : ViewModelBase, IDisposable {
	public IUIObjectService<PostCardListBox> PostsListBoxService => GetService<ITypedUIObjectService>(nameof(PostsListBoxService)).As<PostCardListBox>();
	public IUIObjectService<ButtonPopup> PaginationButtonPopupService => GetService<ITypedUIObjectService>(nameof(PaginationButtonPopupService)).As<ButtonPopup>();


	public IUIObjectService<Grid> DownloadDialogRootService => GetService<ITypedUIObjectService>(nameof(DownloadDialogRootService)).As<Grid>();


	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	public IDialogServiceEx AppSettingsDialog => GetService<IDialogServiceEx>(nameof(AppSettingsDialog));


	public IViewConfigService ViewConfigService { get; } = viewConfigService;
	public IDownloadService DownloadService { get; } = downloadService;
	public IDownloadIndexService DownloadIndexService { get; } = downloadIndexService;

	/// <summary>Bumped when the download index changes so slide-panel badges refresh.</summary>
	public int DownloadIndexVersion {
		get => GetProperty(() => DownloadIndexVersion);
		private set => SetProperty(() => DownloadIndexVersion, value);
	}

	public event TypedEventHandler<PostsViewModel, E621Post?>? CurrentPostChanged;
	public event TypedEventHandler<PostsViewModel, EventArgs>? SelectionChanged;

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
			int clamped = NumberHelper.Clamp(value, 1, int.MaxValue);
			int previous = GetProperty(() => CurrentPage);
			SetProperty(() => CurrentPage, clamped);
			RaisePropertyChanged(() => CanGoLeft);
			if (TabItem != null) {
				TabItem.CurrentPage = clamped;
			}

			if (previous != clamped && TabItem != null) {
				TabItem.ParentViewModel.ScheduleSessionSave();
			}
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
			RaiseSelectionCommandsCanExecuteChanged();
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
		DownloadIndexService.IndexChanged += OnDownloadIndexChanged;
	}

	public void Initialize(PostTabItem postTabItem) => Initialize(postTabItem, autoRefresh: true);

	public void Initialize(PostTabItem postTabItem, bool autoRefresh) {
		TabItem = postTabItem;

		ModuleType = postTabItem.ViewParameter.ModuleType;

		CurrentPage = postTabItem.InitialPage;

		RaisePropertyChanged(() => IsPoolTab);
		RaisePropertyChanged(() => PoolHeaderVisible);

		if (autoRefresh) {
			Refresh();
		}
	}

	public bool IsPoolTab => TabItem?.Kind == PostTabKind.Pool;

	public bool PoolHeaderVisible => IsPoolTab && TabItem?.Pool != null;

	public void Dispose() {
		eventAggregator.GetEvent<AppSettingsChangedEvent>().Unsubscribe(OnAppSettingsChanged);
		DownloadIndexService.IndexChanged -= OnDownloadIndexChanged;
	}

	private void OnAppSettingsChanged(AppSettingsChangedEventArgs args) {
		if (DownloadConfig is not null) {
			DownloadConfig.AppSettingsDownloadFolder = args.Model.DownloadFolderPath;
		}

		DownloadIndexService.EnsureRoot(args.Model.DownloadFolderPath);

		foreach (PostCardControl card in Items) {
			card.GifAutoPlayType = args.Model.GifAutoPlayType;
		}

		RefreshDownloadedBadges();
		downloadPostCommand?.RaiseCanExecuteChanged();
	}

	private void OnDownloadIndexChanged(object? sender, EventArgs e) {
		void Refresh() => RefreshDownloadedBadges();

		if (DispatcherService is { } dispatcher) {
			dispatcher.Invoke(Refresh);
		} else {
			Refresh();
		}
	}

	private bool isRefreshingDownloadedBadges;

	public bool IsPostDownloaded(E621Post? post) {
		if (post == null) {
			return false;
		}

		string site = E621API.GetHost(ModuleType);
		return DownloadIndexService.IsDownloaded(site, post.ID.ToString());
	}

	private void RefreshDownloadedBadges() {
		if (isRefreshingDownloadedBadges) {
			return;
		}

		isRefreshingDownloadedBadges = true;
		try {
			string site = E621API.GetHost(ModuleType);
			foreach (PostCardControl card in Items) {
				card.IsDownloaded = DownloadIndexService.IsDownloaded(site, card.Post.ID.ToString());
			}

			DownloadIndexVersion++;
		} finally {
			isRefreshingDownloadedBadges = false;
		}
	}

	private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}

	private void UpdateMultiSelectingText() {
		if (IsMultiSelecting) {
			MultiSelectingText = $"{Items.Count(x => x.IsSelected)}/{Items.Count}";
		} else {
			MultiSelectingText = string.Empty;
		}
	}

	private void RaiseSelectionCommandsCanExecuteChanged() {
		selectAllCommand?.RaiseCanExecuteChanged();
		toggleSelectionCommand?.RaiseCanExecuteChanged();
		clearSelectionCommand?.RaiseCanExecuteChanged();
		downloadPostCommand?.RaiseCanExecuteChanged();
		selectPostCommand?.RaiseCanExecuteChanged();
		SelectionChanged?.Invoke(this, EventArgs.Empty);
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
		if (SelectedItems.IsNotEmpty()) {
			DownloadSelected();
		} else if (Items.IsNotEmpty()) {
			DownloadCurrentPage();
		}
	}

	private DelegateCommand? selectAllCommand;
	public IDelegateCommand SelectAllCommand => selectAllCommand ??= new(SelectAll, CanSelectAll);
	private void SelectAll() {
		if (!CanSelectAll()) {
			return;
		}

		EnsureMultiSelectingWithoutClearing();
		foreach (PostCardControl item in Items) {
			item.IsSelected = true;
		}
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}
	private bool CanSelectAll() => Items.IsNotEmpty();

	private DelegateCommand? toggleSelectionCommand;
	public IDelegateCommand ToggleSelectionCommand => toggleSelectionCommand ??= new(ToggleSelection, CanToggleSelection);
	private void ToggleSelection() {
		if (!CanToggleSelection()) {
			return;
		}

		EnsureMultiSelectingWithoutClearing();
		foreach (PostCardControl item in Items) {
			item.IsSelected = !item.IsSelected;
		}
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}
	private bool CanToggleSelection() => IsMultiSelecting && Items.IsNotEmpty();

	private DelegateCommand? clearSelectionCommand;
	public IDelegateCommand ClearSelectionCommand => clearSelectionCommand ??= new(ClearSelection, CanClearSelection);
	private void ClearSelection() {
		if (!CanClearSelection()) {
			return;
		}

		foreach (PostCardControl item in Items) {
			item.IsSelected = false;
		}
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}
	private bool CanClearSelection() => IsMultiSelecting && Items.Any(x => x.IsSelected);

	/// <summary>Turns on multi-select without wiping existing item selection flags.</summary>
	private void EnsureMultiSelectingWithoutClearing() {
		if (IsMultiSelecting) {
			return;
		}

		SetProperty(() => IsMultiSelecting, true);
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}

	private DelegateCommand<E621Post?>? openPostCommand;
	public IDelegateCommand OpenPostCommand => openPostCommand ??= new(OpenPost, CanOpenPost);
	private void OpenPost(E621Post? post) {
		if (CanOpenPost(post)) {
			ViewPostDetailDirect(post);
		}
	}
	private bool CanOpenPost(E621Post? post) => post != null;

	private DelegateCommand<E621Post?>? downloadPostCommand;
	public IDelegateCommand DownloadPostCommand => downloadPostCommand ??= new(DownloadPost, CanDownloadPost);
	private void DownloadPost(E621Post? post) {
		if (!CanDownloadPost(post)) {
			return;
		}

		string folder = appSettingsService.Model.DownloadFolderPath;
		if (folder.IsBlank()) {
			return;
		}

		string? subDirectory = DownloadGroupingHelper.ResolveSubDirectory(
			appSettingsService.Model,
			TabItem.Tags,
			post!);
		string site = E621API.GetHost(ModuleType);
		DownloadService.StartDownloads([new E621PostDownloadable(post!, site, subDirectory)], folder);
	}
	private bool CanDownloadPost(E621Post? post) =>
		post?.File != null
		&& post.File.URL.IsNotBlank()
		&& appSettingsService.Model.DownloadFolderPath.IsNotBlank();

	private DelegateCommand<PostCardControl?>? selectPostCommand;
	public IDelegateCommand SelectPostCommand => selectPostCommand ??= new(SelectPost, CanSelectPost);
	private void SelectPost(PostCardControl? card) {
		if (!CanSelectPost(card)) {
			return;
		}

		EnsureMultiSelectingWithoutClearing();
		card!.IsSelected = true;
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}
	private bool CanSelectPost(PostCardControl? card) => card != null;

	public bool IsPostSelected(E621Post? post) => FindCard(post)?.IsSelected == true;

	public void SetPostSelected(E621Post? post, bool selected) {
		if (FindCard(post) is not { } card) {
			return;
		}

		if (selected) {
			EnsureMultiSelectingWithoutClearing();
		}
		card.IsSelected = selected;
		UpdateMultiSelectingText();
		RaiseSelectionCommandsCanExecuteChanged();
	}

	private PostCardControl? FindCard(E621Post? post) =>
		post == null ? null : Items.FirstOrDefault(x => x.Post.ID == post.ID);

	public ICommand RefreshCommand => new DelegateCommand(Refresh);
	private async void Refresh() {
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
			CancellationToken token = paginationLoadingCts.Token;

			int pageLimit = 75;
			E621Post[] posts;

			if (TabItem.Kind == PostTabKind.Pool && TabItem.PoolId is int poolId) {
				posts = await LoadPoolPageAsync(poolId, pageLimit, token);
			} else {
				_ = Task.Run(async () => {
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

				posts = await TabItem.Api.GetPostsByTagsAsync(new E621PostParameters() {
					Tags = TabItem.Tags,
					Page = CurrentPage,
					PageLimit = pageLimit,
				}, token);
			}

			token.ThrowIfCancellationRequested();

			foreach (E621Post post in posts) {
				if (post.HasNoValidURLs()) {
					continue;
				}
				PostCardControl card = new(this, ViewConfigService, post);
				card.IsDownloaded = IsPostDownloaded(post);
				Items.Add(card);
				TabItem.Posts.Add(post);
			}

			PostsListBoxService.Object.Items.Refresh();
			DownloadIndexVersion++;
			RaisePropertyChanged(() => PoolHeaderVisible);

			TabItem.LoadingStatus.Done();
		} catch (OperationCanceledException) {
			TabItem.LoadingStatus.Done();
		} catch (Exception ex) {
			TabItem.LoadingStatus.Error(ex.Message);
		}
	}

	private async Task<E621Post[]> LoadPoolPageAsync(int poolId, int pageLimit, CancellationToken token) {
		E621Pool? pool = TabItem.Pool;
		if (pool == null || pool.ID != poolId) {
			pool = await TabItem.Api.GetPoolAsync(poolId.ToString(), token);
			if (pool == null) {
				MaxPage = 1;
				return [];
			}
			TabItem.Pool = pool;
		}

		int[] ids = pool.PostIDs?.ToArray() ?? [];
		int maxPage = Math.Max(1, (int)Math.Ceiling(ids.Length / (double)pageLimit));
		MaxPage = maxPage;
		if (CurrentPage > maxPage) {
			CurrentPage = maxPage;
		}

		int skip = (CurrentPage - 1) * pageLimit;
		int[] pageIds = ids.Skip(skip).Take(pageLimit).ToArray();
		if (pageIds.Length == 0) {
			return [];
		}

		return await TabItem.Api.GetPostsByIdsAsync(pageIds, token);
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
		set {
			if (SetProperty(() => AppSettingsDownloadFolder, value)) {
				RaiseDestinationFolder();
			}
		}
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

	private AsyncCommand? confirmCommand;
	public IDelegateCommand ConfirmCommand => confirmCommand ??= new(Confirm, CanConfirm);
	protected async Task Confirm() {
		try {
			cts = new CancellationTokenSource();
			CancellationToken token = cts.Token;

			LoadingStatus_GetPosts.Initialize("Loading Posts");

			//await Task.Delay(4000, token);
			IEnumerable<E621Post> posts = await GetPostsAsync(token);
			AppSettingsModel settings = AppSettingsService.Instance.Model;
			string site = E621API.GetHost(ParentViewModel.ModuleType);
			IEnumerable<E621PostDownloadable> postsDownloadable = posts
				.Select(x => new E621PostDownloadable(
					x,
					site,
					DownloadGroupingHelper.ResolveSubDirectory(settings, Tags, x)))
				.Where(x => x.CanDownload);

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

file sealed class E621PostDownloadable(E621Post post, string site, string? subDirectory = null) : IDownloadable, INameTemplateItem {
	public string DownloadUrl => post.File!.URL!;
	public string TargetFileName => NameTemplateHandler.GenerateFilename(
		this,
		AppSettingsService.Instance.Model.FileNameTemplate);
	public string? PreviewUrl => post.Preview?.URL;
	public string? SubDirectory => subDirectory;
	public bool CanDownload => post.File != null && post.File.URL.IsNotBlank();

	public string? IndexSite => site;
	public string? IndexItemId => post.ID.ToString();
	public string? IndexMd5 => post.File?.Md5;

	public string Site => site;
	public string Id => post.ID.ToString();
	public string Md5 => post.File?.Md5 ?? string.Empty;
	public IEnumerable<string> Authors => DownloadGroupingHelper.GetAuthorsForFileName(post);
	public string Extension => post.File?.Ext ?? string.Empty;
}
