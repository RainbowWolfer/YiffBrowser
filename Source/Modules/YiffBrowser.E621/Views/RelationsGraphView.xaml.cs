using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views;

internal partial class RelationsGraphView : UserControl, IPostTabContent {
	private readonly RelationsGraphViewModel viewModel;

	private const double MinScale = 0.15;
	private const double MaxScale = 4.0;
	private const double ZoomStep = 1.12;
	private const double GridSpacing = 48;
	private const double PanDragThreshold = 3;

	private bool isPanning;
	private bool panMoved;
	private Point panStartScreen;
	private double panStartTranslateX;
	private double panStartTranslateY;
	private bool pendingFit;

	private RelationGraphNodeVm? pressedNode;
	private Point pressedNodeScreen;

	public RelationsGraphView(PostTabItem postTabItem) {
		InitializeComponent();

		viewModel = IoC.Resolve<RelationsGraphViewModel>()!;
		viewModel.LayoutReady += OnLayoutReady;
		viewModel.Initialize(postTabItem);
		DataContext = viewModel;

		Loaded += (_, _) => {
			RedrawGrid();
			if (pendingFit || viewModel.Nodes.Count > 0) {
				FitToView();
			}
		};
	}

	public int CurrentPage => 1;

	public void RefreshPosts() {
		if (viewModel.RefreshCommand.CanExecute(null)) {
			viewModel.RefreshCommand.Execute(null);
		}
	}

	public void Dispose() {
		viewModel.LayoutReady -= OnLayoutReady;
		viewModel.Dispose();
	}

	private void OnLayoutReady() {
		pendingFit = true;
		if (IsLoaded && Viewport.ActualWidth > 0 && Viewport.ActualHeight > 0) {
			FitToView();
		}
	}

	private void NodeCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton != MouseButton.Left) {
			return;
		}
		if (sender is FrameworkElement { DataContext: RelationGraphNodeVm node }) {
			pressedNode = node;
			pressedNodeScreen = e.GetPosition(Viewport);
		}
	}

	private void NodeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
		RelationGraphNodeVm? node = pressedNode;
		pressedNode = null;

		// Ignore if this release finishes a pan, or the pointer dragged off the click.
		if (node == null || isPanning || panMoved) {
			return;
		}
		if ((e.GetPosition(Viewport) - pressedNodeScreen).LengthSquared > PanDragThreshold * PanDragThreshold) {
			return;
		}
		if (sender is not FrameworkElement { DataContext: RelationGraphNodeVm upNode } || upNode != node) {
			return;
		}

		viewModel.OpenNode(node);
		e.Handled = true;
	}

	private void FitView_Click(object sender, RoutedEventArgs e) => FitToView();

	private void ResetZoom_Click(object sender, RoutedEventArgs e) {
		double oldScale = WorldScale.ScaleX;
		if (oldScale <= 0) {
			oldScale = 1;
		}

		Point center = new(Viewport.ActualWidth / 2, Viewport.ActualHeight / 2);
		double worldX = (center.X - WorldTranslate.X) / oldScale;
		double worldY = (center.Y - WorldTranslate.Y) / oldScale;
		SetTransform(1, center.X - worldX, center.Y - worldY);
		RedrawGrid();
	}

	private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e) {
		RedrawGrid();
		if (pendingFit) {
			FitToView();
		}
	}

	private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e) {
		Viewport.Focus();

		double factor = e.Delta > 0 ? ZoomStep : 1 / ZoomStep;
		double oldScale = WorldScale.ScaleX;
		double newScale = Math.Clamp(oldScale * factor, MinScale, MaxScale);
		if (Math.Abs(newScale - oldScale) < 0.0001) {
			e.Handled = true;
			return;
		}

		Point mouse = e.GetPosition(Viewport);
		double worldX = (mouse.X - WorldTranslate.X) / oldScale;
		double worldY = (mouse.Y - WorldTranslate.Y) / oldScale;

		SetTransform(newScale, mouse.X - worldX * newScale, mouse.Y - worldY * newScale);
		RedrawGrid();
		e.Handled = true;
	}

	private void Viewport_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
		Viewport.Focus();

		bool middle = e.ChangedButton == MouseButton.Middle;
		bool leftOnEmpty = e.ChangedButton == MouseButton.Left && IsEmptyViewportHit(e.OriginalSource);

		if (!middle && !leftOnEmpty) {
			return;
		}

		isPanning = true;
		panMoved = false;
		panStartScreen = e.GetPosition(Viewport);
		panStartTranslateX = WorldTranslate.X;
		panStartTranslateY = WorldTranslate.Y;
		Viewport.CaptureMouse();
		Viewport.Cursor = Cursors.SizeAll;
		e.Handled = true;
	}

	private void Viewport_PreviewMouseMove(object sender, MouseEventArgs e) {
		if (!isPanning) {
			return;
		}

		Point current = e.GetPosition(Viewport);
		Vector delta = current - panStartScreen;
		if (!panMoved && delta.Length >= PanDragThreshold) {
			panMoved = true;
		}

		SetTranslate(panStartTranslateX + delta.X, panStartTranslateY + delta.Y);
		RedrawGrid();
		e.Handled = true;
	}

	private void Viewport_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
		if (!isPanning) {
			return;
		}

		EndPan();
		e.Handled = true;
	}

	private void Viewport_MouseLeave(object sender, MouseEventArgs e) {
		if (isPanning && !Viewport.IsMouseCaptured) {
			EndPan();
		}
	}

	private void EndPan() {
		isPanning = false;
		// Must clear this: leaving it true permanently swallowed every later node click.
		panMoved = false;
		pressedNode = null;
		if (Viewport.IsMouseCaptured) {
			Viewport.ReleaseMouseCapture();
		}
		Viewport.Cursor = Cursors.Arrow;
	}

	/// <summary>
	/// Left-drag pans only on empty chrome (viewport / world / graph host), never on a node card.
	/// </summary>
	private bool IsEmptyViewportHit(object? source) {
		if (source is not DependencyObject current) {
			return false;
		}

		while (current != null) {
			if (current is FrameworkElement { DataContext: RelationGraphNodeVm }) {
				return false;
			}
			if (ReferenceEquals(current, Viewport)
				|| ReferenceEquals(current, World)
				|| ReferenceEquals(current, GraphHost)
				|| ReferenceEquals(current, GridCanvas)) {
				return true;
			}
			current = VisualTreeHelper.GetParent(current);
		}

		return false;
	}

	private void FitToView() {
		double graphW = viewModel.GraphWidth;
		double graphH = viewModel.GraphHeight;
		double viewW = Viewport.ActualWidth;
		double viewH = Viewport.ActualHeight;

		if (graphW <= 0 || graphH <= 0 || viewW <= 0 || viewH <= 0) {
			return;
		}

		const double padding = 48;
		double scale = Math.Min((viewW - padding) / graphW, (viewH - padding) / graphH);
		scale = Math.Clamp(scale, MinScale, MaxScale);

		double tx = (viewW - graphW * scale) / 2;
		double ty = (viewH - graphH * scale) / 2;
		SetTransform(scale, tx, ty);
		RedrawGrid();
		pendingFit = false;
	}

	private void SetTransform(double scale, double translateX, double translateY) {
		WorldScale.ScaleX = scale;
		WorldScale.ScaleY = scale;
		WorldTranslate.X = translateX;
		WorldTranslate.Y = translateY;
	}

	private void SetTranslate(double translateX, double translateY) {
		WorldTranslate.X = translateX;
		WorldTranslate.Y = translateY;
	}

	/// <summary>
	/// Dot grid in world space (lives under <see cref="World"/>). Enough tiles to cover the
	/// visible viewport plus a margin so panning does not flash empty edges.
	/// </summary>
	private void RedrawGrid() {
		if (GridCanvas == null || Viewport == null || WorldScale == null || WorldTranslate == null) {
			return;
		}

		GridCanvas.Children.Clear();

		double scale = WorldScale.ScaleX;
		if (scale <= 0 || Viewport.ActualWidth <= 0 || Viewport.ActualHeight <= 0) {
			return;
		}

		Brush brush = TryBrush("BorderBrush", Brushes.Gray).Clone();
		brush.Opacity = 0.22;
		if (brush.CanFreeze) {
			brush.Freeze();
		}

		double worldLeft = -WorldTranslate.X / scale;
		double worldTop = -WorldTranslate.Y / scale;
		double worldRight = worldLeft + Viewport.ActualWidth / scale;
		double worldBottom = worldTop + Viewport.ActualHeight / scale;

		double pad = GridSpacing * 2;
		int startX = (int)Math.Floor((worldLeft - pad) / GridSpacing);
		int endX = (int)Math.Ceiling((worldRight + pad) / GridSpacing);
		int startY = (int)Math.Floor((worldTop - pad) / GridSpacing);
		int endY = (int)Math.Ceiling((worldBottom + pad) / GridSpacing);

		int countX = Math.Max(0, endX - startX + 1);
		int countY = Math.Max(0, endY - startY + 1);
		int step = 1;
		const int maxDots = 2500;
		if (countX * countY > maxDots) {
			step = (int)Math.Ceiling(Math.Sqrt((double)countX * countY / maxDots));
		}

		double radius = Math.Max(0.8, 1.2 / scale);

		for (int ix = startX; ix <= endX; ix += step) {
			for (int iy = startY; iy <= endY; iy += step) {
				Ellipse dot = new() {
					Width = radius * 2,
					Height = radius * 2,
					Fill = brush,
					IsHitTestVisible = false,
				};
				Canvas.SetLeft(dot, ix * GridSpacing - radius);
				Canvas.SetTop(dot, iy * GridSpacing - radius);
				GridCanvas.Children.Add(dot);
			}
		}
	}

	private static Brush TryBrush(string key, Brush fallback) {
		try {
			if (Application.Current?.TryFindResource(key) is Brush brush) {
				return brush;
			}
		} catch {
			// ignore
		}
		return fallback;
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e) {
		base.OnPreviewKeyDown(e);
		if (e.Key == Key.Home) {
			FitToView();
			e.Handled = true;
		} else if ((e.Key == Key.D0 || e.Key == Key.NumPad0) && Keyboard.Modifiers == ModifierKeys.Control) {
			ResetZoom_Click(this, new RoutedEventArgs());
			e.Handled = true;
		}
	}
}

internal class RelationsGraphViewModel : ViewModelBase, IDisposable {

	public event Action? LayoutReady;

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
				LayoutReady?.Invoke();
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

internal sealed class RelationGraphNodeVm : INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;

	public RelationGraphNodeVm(RelationTreeNode node, int seedId) {
		Post = node.Post;
		Depth = node.Depth;
		IsSeed = node.Post.ID == seedId;
		HasParent = node.Depth > 0;
		LoadPreview();
	}

	private void LoadPreview() {
		string? url = Post.Preview?.URL;
		if (url.IsBlank()) {
			return;
		}

		BitmapCacheItem item = BitmapCacheService.Get(url, E621MediaCacheKeys.For(Post, E621MediaCacheKeys.Preview, url));
		item.Updated += PreviewUpdated;
		item.Initialize();
		if (item.HasCompleted && item.Image != null) {
			item.Updated -= PreviewUpdated;
			PreviewImage = item.Image;
		}
	}

	private void PreviewUpdated(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!args.HasCompleted && !args.HasError) {
			return;
		}

		sender.Updated -= PreviewUpdated;
		if (args.HasCompleted && sender.Image != null) {
			PreviewImage = sender.Image;
		}
	}

	public E621Post Post { get; }
	public int Depth { get; }
	public bool IsSeed { get; }
	public bool HasParent { get; }

	public double X { get; set; }
	public double Y { get; set; }

	public string Title => $"#{Post.ID}";
	public string? PreviewUrl => Post.Preview?.URL;

	public BitmapImage? PreviewImage {
		get;
		private set {
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewImage)));
		}
	}

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
