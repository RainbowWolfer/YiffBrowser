using BaseFramework.Enums;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YB.E621.Controls;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views;

public partial class PostsView : UserControl {
	public PostsView() {
		InitializeComponent();
	}

	private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
		//e.Handled = true;
	}

	private void TextBlock_DragLeave(object sender, DragEventArgs e) {

	}
}

public class PostsViewModel : ViewModelBase {
	public const double ItemWidth = 396;
	public const double ItemHeight = 50;

	public ObservableCollection<PostCardControl> Items { get; } = [];
	public ObservableCollection<PostCardControl> SelectedItems { get; } = [];

	public E621API Api { get; }

	public ModuleType SiteType { get; }
	public string[] Tags { get; }

	public bool IsLoading {
		get => GetProperty(() => IsLoading);
		set => SetProperty(() => IsLoading, value);
	}

	public int CurrentPage {
		get => GetProperty(() => CurrentPage);
		set {
			SetProperty(() => CurrentPage, Math.Clamp(value, 1, int.MaxValue));
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

	public PostDetailViewModel PostDetailViewModel { get; }

	public PostsViewModel(ModuleType moduleType, string[] tags) {
		Api = E621API.GetAPI(moduleType);
		PostDetailViewModel = new PostDetailViewModel(moduleType);

		SiteType = moduleType;
		Tags = tags;

		SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

		CurrentPage = 1;
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


	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		Refresh();
	}

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
		if (IsLoading) {
			return;
		}

		IsLoading = true;
		IsMultiSelecting = false;

		Items.Clear();
		E621Post[] posts = await Api.GetPostsByTagsAsync(new E621PostParameters() {
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
	public ICommand ViewPostDetailCommandDirect => new DelegateCommand<E621Post?>(ViewPostDetailDirect);

	private void ViewPostDetail(E621Post? post) {
		if (IsMultiSelecting) {
			return;
		}
		ViewPostDetailDirect(post);
	}

	private void ViewPostDetailDirect(E621Post? post) {
		PostDetailViewModel.Post = post;
		PostDetailViewModel.Focus();
	}

	public ICommand QuitPostDetailViewCommand => new DelegateCommand(QuitPostDetailView);

	private void QuitPostDetailView() {
		PostDetailViewModel.Back();
	}

}
