using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.ViewModels;

internal class PostTabItem : BindableBase, IDisposable {

	public E621API Api { get; }

	public E621MainViewModel ParentViewModel { get; }
	public ViewParameter ViewParameter { get; }

	public string[] Tags { get; }

	public int InitialPage { get; }

	public PostsView View { get; }

	public ObservableCollection<E621Post> Posts { get; } = [];

	/// <summary>Up to four preview URLs for the tabs-manage 2×2 card mosaic.</summary>
	public ObservableCollection<string?> CoverPreviewUrls { get; } = [];

	public string? CoverImageUrl {
		get => GetProperty(() => CoverImageUrl);
		private set => SetProperty(() => CoverImageUrl, value);
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

	public PostTabItem(E621MainViewModel parentViewModel, string[] tags, int initialPage = 1) {
		ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		ViewParameter = parentViewModel.ViewParameter ?? throw new ArgumentNullException(nameof(ViewParameter));

		Api = E621API.GetAPI(ViewParameter.ModuleType);

		ParentViewModel = parentViewModel;
		Tags = tags ?? [];
		InitialPage = Math.Max(1, initialPage);
		CurrentPage = InitialPage;

		for (int i = 0; i < 4; i++) {
			CoverPreviewUrls.Add(null);
		}

		Posts.CollectionChanged += OnPostsCollectionChanged;

		View = new PostsView(this);
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

	public void Refresh() => View.RefreshPosts();

	public void Dispose() {
		Posts.CollectionChanged -= OnPostsCollectionChanged;
		View.Dispose();
	}
}
