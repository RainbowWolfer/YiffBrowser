using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.ViewModels;

internal class PostTabItem : BindableBase, IDisposable {

	public E621API Api { get; }

	public E621MainViewModel ParentViewModel { get; }
	public ViewParameter ViewParameter { get; }

	public PostTabKind Kind { get; }

	public string[] Tags { get; }

	public int? PoolId { get; }

	public int? RelationsRootPostId { get; }

	public int InitialPage { get; }

	public FrameworkElement View { get; }

	private readonly IPostTabContent content;

	public ObservableCollection<E621Post> Posts { get; } = [];

	/// <summary>Up to four preview URLs for the tabs-manage 2×2 card mosaic.</summary>
	public ObservableCollection<string?> CoverPreviewUrls { get; } = [];

	public string? CoverImageUrl {
		get => GetProperty(() => CoverImageUrl);
		private set => SetProperty(() => CoverImageUrl, value);
	}

	public string Title {
		get => GetProperty(() => Title);
		set => SetProperty(() => Title, value);
	}

	public E621Pool? Pool {
		get => GetProperty(() => Pool);
		set {
			SetProperty(() => Pool, value);
			if (Kind == PostTabKind.Pool && PoolId is int id) {
				Title = PostTabTitleHelper.ForPool(id, value?.Name);
			}
		}
	}

	public LoadingStatus LoadingStatus { get; } = new();

	/// <summary>Selection flag used only on the tabs-manage grid.</summary>
	public bool IsManageSelected {
		get => GetProperty(() => IsManageSelected);
		set => SetProperty(() => IsManageSelected, value);
	}

	public int CurrentPage {
		get => GetProperty(() => CurrentPage);
		set => SetProperty(() => CurrentPage, Math.Max(1, value));
	}

	public PostTabItem(E621MainViewModel parentViewModel, string[] tags, int initialPage = 1)
		: this(parentViewModel, PostTabKind.Search, tags, poolId: null, relationsRootPostId: null, initialPage) {
	}

	private PostTabItem(
		E621MainViewModel parentViewModel,
		PostTabKind kind,
		string[] tags,
		int? poolId,
		int? relationsRootPostId,
		int initialPage
	) {
		ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		ViewParameter = parentViewModel.ViewParameter ?? throw new ArgumentNullException(nameof(ViewParameter));

		Api = E621API.GetAPI(ViewParameter.ModuleType);

		Kind = kind;
		Tags = tags ?? [];
		PoolId = poolId;
		RelationsRootPostId = relationsRootPostId;
		InitialPage = Math.Max(1, initialPage);
		CurrentPage = InitialPage;

		Title = kind switch {
			PostTabKind.Pool when poolId is int pid => PostTabTitleHelper.ForPool(pid),
			PostTabKind.Relations when relationsRootPostId is int rid => PostTabTitleHelper.ForRelations(rid),
			_ => PostTabTitleHelper.FromTags(Tags),
		};

		for (int i = 0; i < 4; i++) {
			CoverPreviewUrls.Add(null);
		}

		Posts.CollectionChanged += OnPostsCollectionChanged;

		content = kind switch {
			PostTabKind.Relations => CreateRelationsContent(),
			_ => CreatePostsContent(),
		};
		View = (FrameworkElement)content;
	}

	public static PostTabItem CreatePool(E621MainViewModel parent, int poolId, int initialPage = 1) {
		return new PostTabItem(parent, PostTabKind.Pool, [$"pool:{poolId}"], poolId, null, initialPage);
	}

	public static PostTabItem CreateRelations(E621MainViewModel parent, int rootPostId) {
		return new PostTabItem(parent, PostTabKind.Relations, [$"id:{rootPostId}"], null, rootPostId, 1);
	}

	private IPostTabContent CreatePostsContent() {
		PostsView view = new(this);
		return view;
	}

	private IPostTabContent CreateRelationsContent() {
		RelationsGraphView view = new(this);
		return view;
	}

	private void OnPostsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		UpdateCoverPreviews();
	}

	private void UpdateCoverPreviews() {
		for (int i = 0; i < 4; i++) {
			string? url = null;
			if (i < Posts.Count) {
				url = Posts[i].Preview?.URL;
				if (string.IsNullOrWhiteSpace(url)) {
					url = null;
				}
			}

			if (!string.Equals(CoverPreviewUrls[i], url, StringComparison.Ordinal)) {
				CoverPreviewUrls[i] = url;
			}
		}

		CoverImageUrl = CoverPreviewUrls.FirstOrDefault(u => u != null);
	}

	public void Refresh() => content.RefreshPosts();

	public int GetContentPage() => content.CurrentPage;

	public void Dispose() {
		Posts.CollectionChanged -= OnPostsCollectionChanged;
		content.Dispose();
	}
}
