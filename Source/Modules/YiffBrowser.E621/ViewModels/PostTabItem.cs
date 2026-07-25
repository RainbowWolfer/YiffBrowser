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

	public PostsView View { get; }

	public ObservableCollection<E621Post> Posts { get; } = [];

	public string? CoverImageUrl {
		get => GetProperty(() => CoverImageUrl);
		private set => SetProperty(() => CoverImageUrl, value);
	}

	public LoadingStatus LoadingStatus { get; } = new();

	public PostTabItem(E621MainViewModel parentViewModel, string[] tags) {
		ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		ViewParameter = parentViewModel.ViewParameter ?? throw new ArgumentNullException(nameof(ViewParameter));

		Api = E621API.GetAPI(ViewParameter.ModuleType);

		ParentViewModel = parentViewModel;
		Tags = tags ?? [];

		Posts.CollectionChanged += OnPostsCollectionChanged;

		View = new PostsView(this);
	}

	private void OnPostsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		string? url = Posts.Count > 0 ? Posts[0].Preview?.URL : null;
		CoverImageUrl = string.IsNullOrWhiteSpace(url) ? null : url;
	}

	public void Dispose() {
		Posts.CollectionChanged -= OnPostsCollectionChanged;
		View.Dispose();
	}
}
