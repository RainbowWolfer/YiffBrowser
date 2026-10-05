using YiffBrowser.BaseFramework.ViewModels;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.Views;

public partial class PostDetailDockView : UserControl, INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



	public ModuleType? ModuleType {
		get => (ModuleType?)GetValue(ModuleTypeProperty);
		set => SetValue(ModuleTypeProperty, value);
	}

	public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.Register(
		nameof(ModuleType),
		typeof(ModuleType?),
		typeof(PostDetailDockView),
		new PropertyMetadata(null, OnModuleTypeChanged)
	);

	private static void OnModuleTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostDetailDockView view) {
			view.LoadComments();
		}
	}

	public E621Post Post {
		get => (E621Post)GetValue(PostProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
		nameof(Post),
		typeof(E621Post),
		typeof(PostDetailDockView),
		new PropertyMetadata(null, OnPostChanged)
	);

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostDetailDockView view) {
			view.Raise(nameof(Description));
			view.Raise(nameof(SourceTitle));
			view.Raise(nameof(SourceURLs));
			view.Raise(nameof(HasRelations));
			view.Raise(nameof(HasPools));
			view.UpdateRelations();
			view.LoadComments();
		}
	}

	public string Description => Post?.Description.NotBlankCheck() ?? "No Description";
	public string[] SourceURLs => Post?.Sources?.ToArray() ?? [];

	public string SourceTitle {
		get {
			if (SourceURLs.IsEmpty()) {
				return "No Source";
			} else if (SourceURLs.Length == 1) {
				return "Source";
			} else {
				return "Sources";
			}
		}
	}

	public ObservableCollection<E621CommentViewModel> Comments { get; } = [];
	public ObservableCollection<RelationNeighborItem> ParentItems { get; } = [];
	public ObservableCollection<RelationNeighborItem> ChildItems { get; } = [];
	public ObservableCollection<PoolChipItem> PoolItems { get; } = [];

	public bool HasRelations => ParentItems.Count > 0 || ChildItems.Count > 0;
	public bool HasPools => PoolItems.Count > 0;

	public LoadingStatus LoadingStatus { get; } = new();
	public LoadingStatus RelationsLoadingStatus { get; } = new();

	private CancellationTokenSource? comment_cts;
	private CancellationTokenSource? relations_cts;

	private async void LoadComments() {
		if (ModuleType is null || Post is null) {
			return;
		}

		comment_cts?.Cancel();

		CancellationTokenSource newCts = new();
		comment_cts = newCts;
		CancellationToken token = newCts.Token;

		try {
			LoadingStatus.Initialize();

			token.ThrowIfCancellationRequested();
			foreach (E621CommentViewModel item in Comments) {
				item.Dispose();
			}
			Comments.Clear();

			E621Comment[] comments = await E621API.GetAPI(ModuleType.Value).GetCommentsAsync(Post.ID, token);

			token.ThrowIfCancellationRequested();

			foreach (E621Comment item in comments) {
				Comments.Add(new E621CommentViewModel(item, ModuleType.Value));
			}

			foreach (E621CommentViewModel item in Comments) {
				item.StartLoading();
			}

			if (!token.IsCancellationRequested) {
				LoadingStatus.Done();
			}

		} catch (OperationCanceledException) {

		} catch (Exception ex) {
			if (!token.IsCancellationRequested) {
				LoadingStatus.Error(ex.Message);
				Debug.WriteLine(ex);
			}
		} finally {

		}
	}

	private async void UpdateRelations() {
		ParentItems.Clear();
		ChildItems.Clear();
		PoolItems.Clear();
		Raise(nameof(HasRelations));
		Raise(nameof(HasPools));

		// Snapshot before any await: Post is a DP and can become null while we load
		// (switch post / close detail), which used to NRE on the continuation.
		E621Post? post = Post;
		if (ModuleType is null || post is null) {
			return;
		}

		foreach (int poolId in post.Pools ?? []) {
			PoolItems.Add(new PoolChipItem(poolId));
		}
		Raise(nameof(HasPools));

		relations_cts?.Cancel();
		CancellationTokenSource cts = new();
		relations_cts = cts;
		CancellationToken token = cts.Token;

		try {
			RelationsLoadingStatus.Initialize();
			E621API api = E621API.GetAPI(ModuleType.Value);

			int? parentId = post.Relationships?.ParentId is int pid and > 0 ? pid : null;
			List<int> childIds = (post.Relationships?.Children ?? [])
				.Where(id => id is int and > 0)
				.Select(id => id!.Value)
				.ToList();

			List<int> ids = [];
			if (parentId is int p) {
				ids.Add(p);
			}
			ids.AddRange(childIds);

			if (ids.Count == 0) {
				RelationsLoadingStatus.Done();
				Raise(nameof(HasRelations));
				return;
			}

			E621Post[] posts = await api.GetPostsByIdsAsync(ids, token);
			token.ThrowIfCancellationRequested();

			// User already moved on to another post — drop this result.
			if (!ReferenceEquals(Post, post)) {
				return;
			}

			Dictionary<int, E621Post> map = posts
				.GroupBy(p => p.ID)
				.ToDictionary(g => g.Key, g => g.First());

			if (parentId is int resolvedParent && map.TryGetValue(resolvedParent, out E621Post? parentPost)) {
				ParentItems.Add(new RelationNeighborItem(parentPost, "Parent"));
			}
			foreach (int cid in childIds) {
				if (map.TryGetValue(cid, out E621Post? childPost)) {
					ChildItems.Add(new RelationNeighborItem(childPost, "Child"));
				}
			}

			RelationsLoadingStatus.Done();
			Raise(nameof(HasRelations));
		} catch (OperationCanceledException) {
		} catch (Exception ex) {
			if (!token.IsCancellationRequested) {
				RelationsLoadingStatus.Error(ex.Message);
				Debug.WriteLine(ex);
			}
		}
	}

	private E621MainViewModel? GetMainViewModel() {
		if (DataContext is PostDetailViewModel detail) {
			return detail.ParentViewModel?.TabItem.ParentViewModel;
		}

		DependencyObject? current = this;
		while (current != null) {
			if (current is FrameworkElement { DataContext: PostDetailViewModel vm }) {
				return vm.ParentViewModel?.TabItem.ParentViewModel;
			}
			current = VisualTreeHelper.GetParent(current);
		}
		return null;
	}

	private void OpenRelationsGraph_Click(object sender, RoutedEventArgs e) {
		if (Post == null) {
			return;
		}
		GetMainViewModel()?.OpenRelationsTab(Post.ID);
	}

	private void RelationNeighbor_Click(object sender, MouseButtonEventArgs e) {
		if (Post == null) {
			return;
		}
		GetMainViewModel()?.OpenRelationsTab(Post.ID);
	}

	private void PoolChip_Click(object sender, RoutedEventArgs e) {
		if (sender is FrameworkElement { DataContext: PoolChipItem chip }) {
			GetMainViewModel()?.OpenPoolTab(chip.PoolId);
		}
	}

	public PostDetailDockView() {
		InitializeComponent();
	}
}

public sealed class RelationNeighborItem(E621Post post, string role) {
	public E621Post Post { get; } = post;
	public string Role { get; } = role;
	public string Title => $"#{Post.ID}";
	public string? PreviewUrl => Post.Preview?.URL;
}

public sealed class PoolChipItem {
	public int PoolId { get; }
	public string Label => $"Pool #{PoolId}";

	public PoolChipItem(int poolId) {
		PoolId = poolId;
	}
}
