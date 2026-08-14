using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
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
}

internal class RelationsGraphViewModel : ViewModelBase, IDisposable {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<Canvas> GraphCanvasService => GetService<ITypedUIObjectService>(nameof(GraphCanvasService)).As<Canvas>();

	public PostsViewModel? PostsHost {
		get => GetProperty(() => PostsHost);
		private set => SetProperty(() => PostsHost, value);
	}

	public PostTabItem TabItem {
		get => GetProperty(() => TabItem);
		private set => SetProperty(() => TabItem, value);
	}

	public RelationTreeNode? Root {
		get => GetProperty(() => Root);
		private set => SetProperty(() => Root, value);
	}

	public ObservableCollection<RelationGraphNodeVm> Nodes { get; } = [];

	public string StatusText {
		get => GetProperty(() => StatusText);
		set => SetProperty(() => StatusText, value);
	}

	private CancellationTokenSource? loadCts;
	private RelationTreeNode? pendingRoot;

	public void Initialize(PostTabItem postTabItem) {
		TabItem = postTabItem;

		PostsHost = IoC.Resolve<PostsViewModel>()!;
		PostsHost.Initialize(postTabItem, autoRefresh: false);

		Refresh();
	}

	public void Dispose() {
		loadCts?.Cancel();
		PostsHost?.Dispose();
	}

	public ICommand RefreshCommand => new DelegateCommand(Refresh);

	private async void Refresh() {
		if (TabItem.RelationsRootPostId is not int seedId || seedId <= 0) {
			TabItem.LoadingStatus.Error("Missing relations root post id.");
			return;
		}

		if (TabItem.LoadingStatus.ShowLoading) {
			return;
		}

		loadCts?.Cancel();
		loadCts = new CancellationTokenSource();
		CancellationToken token = loadCts.Token;

		TabItem.LoadingStatus.Initialize();
		StatusText = "Loading relationship tree…";
		Nodes.Clear();
		TabItem.Posts.Clear();
		Root = null;
		PostsHost!.CurrentPost = null;

		try {
			RelationTreeBuildResult result = await TabItem.Api.BuildRelationTreeAsync(seedId, token);
			token.ThrowIfCancellationRequested();

			Root = result.Root;
			pendingRoot = result.Root;
			foreach (E621Post post in result.AllPosts) {
				TabItem.Posts.Add(post);
			}

			_ = DispatcherService.Dispatcher.BeginInvoke(() => BuildLayout(pendingRoot), DispatcherPriority.Loaded);
			StatusText = result.Root == null
				? "No relationship tree found."
				: $"{result.AllPosts.Count} related post(s)";

			TabItem.LoadingStatus.Done();
		} catch (OperationCanceledException) {
			TabItem.LoadingStatus.Done();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			StatusText = ex.Message;
			TabItem.LoadingStatus.Error(ex.Message);
		}
	}

	private const double NodeWidth = 120;
	private const double NodeHeight = 140;
	private const double HGap = 24;
	private const double VGap = 48;

	private void BuildLayout(RelationTreeNode? root) {
		Nodes.Clear();
		Canvas? canvas = null;
		try {
			canvas = GraphCanvasService.Object;
		} catch {
			// service may not be ready yet
		}

		canvas?.Children.Clear();
		if (root == null) {
			if (canvas != null) {
				canvas.Width = 0;
				canvas.Height = 0;
			}
			return;
		}

		Dictionary<RelationTreeNode, double> subtreeWidths = [];
		double Measure(RelationTreeNode node) {
			if (node.Children.Count == 0) {
				subtreeWidths[node] = NodeWidth;
				return NodeWidth;
			}
			double width = 0;
			foreach (RelationTreeNode child in node.Children) {
				width += Measure(child);
			}
			width += HGap * (node.Children.Count - 1);
			width = Math.Max(width, NodeWidth);
			subtreeWidths[node] = width;
			return width;
		}

		double totalWidth = Measure(root);
		List<(RelationTreeNode parent, RelationTreeNode child, Point from, Point to)> edges = [];

		void Place(RelationTreeNode node, double left, double top) {
			double subtree = subtreeWidths[node];
			double x = left + (subtree - NodeWidth) / 2;
			RelationGraphNodeVm vm = new(node) {
				X = x,
				Y = top,
			};
			Nodes.Add(vm);

			if (node.Children.Count == 0) {
				return;
			}

			double childLeft = left;
			foreach (RelationTreeNode child in node.Children) {
				double childWidth = subtreeWidths[child];
				Place(child, childLeft, top + NodeHeight + VGap);
				RelationGraphNodeVm? childVm = Nodes.FirstOrDefault(n => n.Post.ID == child.Post.ID);
				if (childVm != null) {
					edges.Add((node, child,
						new Point(x + NodeWidth / 2, top + NodeHeight),
						new Point(childVm.X + NodeWidth / 2, childVm.Y)));
				}
				childLeft += childWidth + HGap;
			}
		}

		Place(root, 16, 16);

		double maxX = Nodes.Count == 0 ? totalWidth : Nodes.Max(n => n.X + NodeWidth) + 16;
		double maxY = Nodes.Count == 0 ? NodeHeight : Nodes.Max(n => n.Y + NodeHeight) + 16;

		if (canvas != null) {
			canvas.Width = Math.Max(maxX, totalWidth + 32);
			canvas.Height = maxY;

			foreach ((RelationTreeNode parent, RelationTreeNode child, Point from, Point to) in edges) {
				_ = parent;
				_ = child;
				PathFigure figure = new() { StartPoint = from };
				double midY = (from.Y + to.Y) / 2;
				figure.Segments.Add(new BezierSegment(
					new Point(from.X, midY),
					new Point(to.X, midY),
					to,
					isStroked: true));
				PathGeometry geometry = new();
				geometry.Figures.Add(figure);
				Path path = new() {
					Data = geometry,
					Stroke = TryBrush("PrimaryTextBrush", Brushes.Gray),
					StrokeThickness = 1.5,
					Opacity = 0.45,
				};
				canvas.Children.Add(path);
			}

			foreach (RelationGraphNodeVm node in Nodes) {
				Border card = CreateNodeCard(node);
				Canvas.SetLeft(card, node.X);
				Canvas.SetTop(card, node.Y);
				canvas.Children.Add(card);
			}
		}
	}

	private Border CreateNodeCard(RelationGraphNodeVm node) {
		Image image = new() {
			Stretch = Stretch.UniformToFill,
			Source = null,
		};
		if (node.Post.Preview?.URL.IsNotBlank() == true) {
			image.Source = new BitmapImage(new Uri(node.Post.Preview.URL!));
		}

		Border preview = new() {
			Width = NodeWidth - 8,
			Height = 96,
			CornerRadius = new CornerRadius(4),
			ClipToBounds = true,
			Child = image,
			Background = TryBrush("SecondaryRegionBrush", Brushes.DimGray),
		};

		TextBlock idText = new() {
			Text = $"#{node.Post.ID}",
			FontWeight = FontWeights.SemiBold,
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(0, 4, 0, 0),
		};

		TextBlock badge = new() {
			Text = node.IsSeed ? "seed" : node.Depth == 0 ? "root" : $"d{node.Depth}",
			FontSize = 11,
			Opacity = 0.7,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		StackPanel stack = new() { Children = { preview, idText, badge } };

		Border card = new() {
			Width = NodeWidth,
			Height = NodeHeight,
			Padding = new Thickness(4),
			CornerRadius = new CornerRadius(6),
			BorderThickness = new Thickness(node.IsSeed ? 2 : 1),
			BorderBrush = TryBrush(node.IsSeed ? "PrimaryBrush" : "BorderBrush", Brushes.Gray),
			Background = TryBrush("RegionBrush", Brushes.White),
			Child = stack,
			Cursor = Cursors.Hand,
			Tag = node,
			ToolTip = $"Post #{node.Post.ID}",
		};

		card.MouseLeftButtonUp += (_, _) => OpenPost(node.Post);
		return card;
	}

	private void OpenPost(E621Post post) {
		if (PostsHost == null) {
			return;
		}
		PostsHost.CurrentPost = post;
	}

	public ICommand QuitDetailCommand => new DelegateCommand(() => {
		if (PostsHost != null) {
			PostsHost.CurrentPost = null;
		}
	});

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
}

internal sealed class RelationGraphNodeVm(RelationTreeNode node) {
	public E621Post Post { get; } = node.Post;
	public bool IsSeed { get; } = node.IsSeed;
	public int Depth { get; } = node.Depth;
	public double X { get; set; }
	public double Y { get; set; }
}
