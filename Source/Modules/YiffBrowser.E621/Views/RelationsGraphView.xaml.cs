using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Models;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

internal partial class RelationsGraphView : UserControl, IPostTabContent {
	private readonly RelationsGraphViewModel viewModel;

	public RelationsGraphView(PostTabItem postTabItem) {
		InitializeComponent();

		viewModel = IoC.Resolve<RelationsGraphViewModel>()!;
		viewModel.Initialize(postTabItem);
		DataContext = viewModel;
	}

	public int CurrentPage => 1;

	public void RefreshPosts() {
		if (viewModel.RefreshCommand.CanExecute(null)) {
			viewModel.RefreshCommand.Execute(null);
		}
	}

	public void Dispose() {
		viewModel.Dispose();
	}

	private void NodeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
		if (sender is FrameworkElement { DataContext: RelationGraphNodeVm node }) {
			viewModel.OpenNode(node);
		}
	}
}

internal class RelationsGraphViewModel : ViewModelBase, IDisposable {

	public RelationsGraphViewModel() {
		// Visibility defaults to Visible (enum 0), which would let the detail overlay
		// cover the graph before anything is loaded.
		GraphVisibility = Visibility.Visible;
		DetailVisibility = Visibility.Collapsed;
		LoadingVisibility = Visibility.Collapsed;
		EmptyVisibility = Visibility.Collapsed;
	}

	/// <summary>Set before the tree loads so the header/status can render immediately.</summary>
	public string HeaderTitle {
		get => GetProperty(() => HeaderTitle);
		private set => SetProperty(() => HeaderTitle, value);
	}

	public string StatusText {
		get => GetProperty(() => StatusText);
		private set => SetProperty(() => StatusText, value);
	}

	public PostsViewModel? PostsHost {
		get => GetProperty(() => PostsHost);
		private set => SetProperty(() => PostsHost, value);
	}

	public PostTabItem TabItem {
		get => GetProperty(() => TabItem);
		private set => SetProperty(() => TabItem, value);
	}

	/// <summary>
	/// Visibility is driven from the view model instead of nested bindings + converters:
	/// a failed binding would leave both layers at their default Visible and the detail
	/// overlay would cover the graph.
	/// </summary>
	public Visibility GraphVisibility {
		get => GetProperty(() => GraphVisibility);
		private set => SetProperty(() => GraphVisibility, value);
	}

	public Visibility DetailVisibility {
		get => GetProperty(() => DetailVisibility);
		private set => SetProperty(() => DetailVisibility, value);
	}

	public Visibility LoadingVisibility {
		get => GetProperty(() => LoadingVisibility);
		private set => SetProperty(() => LoadingVisibility, value);
	}

	public Visibility EmptyVisibility {
		get => GetProperty(() => EmptyVisibility);
		private set => SetProperty(() => EmptyVisibility, value);
	}

	public ObservableCollection<RelationGraphNodeVm> Nodes { get; } = [];
	public ObservableCollection<RelationGraphEdgeVm> Edges { get; } = [];

	public double GraphWidth {
		get => GetProperty(() => GraphWidth);
		private set => SetProperty(() => GraphWidth, value);
	}

	public double GraphHeight {
		get => GetProperty(() => GraphHeight);
		private set => SetProperty(() => GraphHeight, value);
	}

	private CancellationTokenSource? loadCts;

	public void Initialize(PostTabItem postTabItem) {
		TabItem = postTabItem;
		HeaderTitle = postTabItem.Title;

		GraphVisibility = Visibility.Visible;
		DetailVisibility = Visibility.Collapsed;
		LoadingVisibility = Visibility.Collapsed;
		EmptyVisibility = Visibility.Collapsed;

		PostsHost = IoC.Resolve<PostsViewModel>()!;
		PostsHost.Initialize(postTabItem, autoRefresh: false);
		PostsHost.CurrentPostChanged += OnCurrentPostChanged;

		Refresh();
	}

	public void Dispose() {
		loadCts?.Cancel();
		if (PostsHost != null) {
			PostsHost.CurrentPostChanged -= OnCurrentPostChanged;
			PostsHost.Dispose();
		}
	}

	private void OnCurrentPostChanged(PostsViewModel sender, E621Post? args) {
		bool hasPost = args != null;
		DetailVisibility = hasPost ? Visibility.Visible : Visibility.Collapsed;
		GraphVisibility = hasPost ? Visibility.Collapsed : Visibility.Visible;
	}

	public void OpenNode(RelationGraphNodeVm node) {
		if (PostsHost != null) {
			PostsHost.CurrentPost = node.Post;
		}
	}

	private DelegateCommand? refreshCommand;
	public IDelegateCommand RefreshCommand => refreshCommand ??= new(Refresh);

	private async void Refresh() {
		if (TabItem?.RelationsRootPostId is not int seedId || seedId <= 0) {
			StatusText = "Missing relations root post id.";
			EmptyVisibility = Visibility.Visible;
			return;
		}

		loadCts?.Cancel();
		loadCts = new CancellationTokenSource();
		CancellationToken token = loadCts.Token;

		TabItem.LoadingStatus.Initialize();
		LoadingVisibility = Visibility.Visible;
		EmptyVisibility = Visibility.Collapsed;
		StatusText = "Loading relationship tree...";

		ClearGraph();
		TabItem.Posts.Clear();
		if (PostsHost != null) {
			PostsHost.CurrentPost = null;
		}

		try {
			RelationTreeBuildResult result = await TabItem.Api.BuildRelationTreeAsync(seedId, token);
			token.ThrowIfCancellationRequested();

			foreach (E621Post post in result.AllPosts) {
				TabItem.Posts.Add(post);
			}

			BuildLayout(result.Root, seedId);

			if (Nodes.Count == 0) {
				StatusText = "No related posts found.";
				EmptyVisibility = Visibility.Visible;
			} else {
				StatusText = $"{Nodes.Count} related post(s)";
			}

			TabItem.LoadingStatus.Done();
		} catch (OperationCanceledException) {
			// A newer refresh owns the status from here on.
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			StatusText = ex.Message;
			EmptyVisibility = Visibility.Visible;
			TabItem.LoadingStatus.Error(ex.Message);
		} finally {
			if (!token.IsCancellationRequested) {
				LoadingVisibility = Visibility.Collapsed;
			}
		}
	}

	private const double NodeWidth = 132;
	private const double NodeHeight = 156;
	private const double HGap = 20;
	private const double VGap = 44;
	private const double Margin = 20;

	private void ClearGraph() {
		Nodes.Clear();
		Edges.Clear();
		GraphWidth = 0;
		GraphHeight = 0;
	}

	private void BuildLayout(RelationTreeNode? root, int seedId) {
		ClearGraph();
		if (root == null) {
			return;
		}

		Dictionary<RelationTreeNode, double> widths = [];

		double Measure(RelationTreeNode node) {
			if (node.Children.Count == 0) {
				return widths[node] = NodeWidth;
			}

			double width = node.Children.Sum(Measure) + HGap * (node.Children.Count - 1);
			return widths[node] = Math.Max(width, NodeWidth);
		}

		Measure(root);

		void Place(RelationTreeNode node, double left, double top) {
			double x = left + (widths[node] - NodeWidth) / 2;
			Nodes.Add(new RelationGraphNodeVm(node, seedId) {
				X = x,
				Y = top,
			});

			double childLeft = left;
			double childTop = top + NodeHeight + VGap;

			foreach (RelationTreeNode child in node.Children) {
				double childX = childLeft + (widths[child] - NodeWidth) / 2;
				Edges.Add(RelationGraphEdgeVm.Create(
					new Point(x + NodeWidth / 2, top + NodeHeight),
					new Point(childX + NodeWidth / 2, childTop)
				));

				Place(child, childLeft, childTop);
				childLeft += widths[child] + HGap;
			}
		}

		Place(root, Margin, Margin);

		GraphWidth = Nodes.Max(n => n.X + NodeWidth) + Margin;
		GraphHeight = Nodes.Max(n => n.Y + NodeHeight) + Margin;
	}
}

internal sealed class RelationGraphNodeVm {
	public RelationGraphNodeVm(RelationTreeNode node, int seedId) {
		Post = node.Post;
		Depth = node.Depth;
		IsSeed = node.Post.ID == seedId;
		HasParent = node.Depth > 0;
	}

	public E621Post Post { get; }
	public int Depth { get; }
	public bool IsSeed { get; }
	public bool HasParent { get; }

	public double X { get; set; }
	public double Y { get; set; }

	public string Title => $"#{Post.ID}";
	public string? PreviewUrl => Post.Preview?.URL;

	public string Badge => IsSeed
		? "current"
		: HasParent ? $"child · d{Depth}" : "root";

	public string ToolTipText => $"Post #{Post.ID} ({Post.Rating})";

	public Thickness BorderThickness => new(IsSeed ? 2 : 1);
}

internal sealed class RelationGraphEdgeVm {
	public required Geometry Data { get; init; }

	public static RelationGraphEdgeVm Create(Point from, Point to) {
		double midY = (from.Y + to.Y) / 2;

		PathFigure figure = new() { StartPoint = from };
		figure.Segments.Add(new BezierSegment(
			new Point(from.X, midY),
			new Point(to.X, midY),
			to,
			isStroked: true
		));

		PathGeometry geometry = new();
		geometry.Figures.Add(figure);
		geometry.Freeze();

		return new RelationGraphEdgeVm { Data = geometry };
	}
}
