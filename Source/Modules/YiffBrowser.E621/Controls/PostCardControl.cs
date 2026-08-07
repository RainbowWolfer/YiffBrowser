using RW.Common.Data;
using RW.Common.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Models;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.Controls;

internal class PostCardControl : ContentControl, IVariableSizedGridItem, IDisposable {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public bool IsSelected {
		get => (bool)GetValue(IsSelectedProperty);
		set => SetValue(IsSelectedProperty, value);
	}

	public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
		nameof(IsSelected),
		typeof(bool),
		typeof(PostCardControl),
		new PropertyMetadata(false, OnIsSelectedChanged)
	);

	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (e.NewValue is true) {

		}
	}

	public E621Post Post {
		get => (E621Post)GetValue(PostProperty.DependencyProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyPropertyKey PostProperty = DependencyProperty.RegisterReadOnly(
		nameof(Post),
		typeof(E621Post),
		typeof(PostCardControl),
		new PropertyMetadata(null)
	);

	public PostLoadingStatus LoadingStatus {
		get => (PostLoadingStatus)GetValue(LoadingStatusProperty);
		private set => SetValue(LoadingStatusPropertyKey, value);
	}

	public static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingStatus),
		typeof(PostLoadingStatus),
		typeof(PostCardControl),
		new PropertyMetadata(PostLoadingStatus.NotStarted)
	);

	public static readonly DependencyProperty LoadingStatusProperty = LoadingStatusPropertyKey.DependencyProperty;


	public double LoadingProgress {
		get => (double)GetValue(LoadingProgressProperty);
		private set => SetValue(LoadingProgressPropertyKey, value);
	}

	public static readonly DependencyPropertyKey LoadingProgressPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingProgress),
		typeof(double),
		typeof(PostCardControl),
		new PropertyMetadata(0d)
	);

	public static readonly DependencyProperty LoadingProgressProperty = LoadingProgressPropertyKey.DependencyProperty;



	public GifImage? GifImage {
		get => (GifImage)GetValue(GifImageProperty);
		set => SetValue(GifImageProperty, value);
	}

	public static readonly DependencyProperty GifImageProperty = DependencyProperty.Register(
		nameof(GifImage),
		typeof(GifImage),
		typeof(PostCardControl),
		new PropertyMetadata(null)
	);

	public BitmapImage? BitmapImage {
		get => (BitmapImage)GetValue(BitmapImageProperty);
		set => SetValue(BitmapImageProperty, value);
	}

	public static readonly DependencyProperty BitmapImageProperty = DependencyProperty.Register(
		nameof(BitmapImage),
		typeof(BitmapImage),
		typeof(PostCardControl),
		new PropertyMetadata(null)
	);


	public GifAutoPlayType GifAutoPlayType {
		get => (GifAutoPlayType)GetValue(GifAutoPlayTypeProperty);
		set => SetValue(GifAutoPlayTypeProperty, value);
	}

	public static readonly DependencyProperty GifAutoPlayTypeProperty = DependencyProperty.Register(
		nameof(GifAutoPlayType),
		typeof(GifAutoPlayType),
		typeof(PostCardControl),
		new PropertyMetadata(GifAutoPlayType.WhenMouseOver)
	);


	public bool IsDownloaded {
		get => (bool)GetValue(IsDownloadedProperty);
		set => SetValue(IsDownloadedProperty, value);
	}

	public static readonly DependencyProperty IsDownloadedProperty = DependencyProperty.Register(
		nameof(IsDownloaded),
		typeof(bool),
		typeof(PostCardControl),
		new PropertyMetadata(false)
	);


	public int ColSpan {
		get => (int)GetValue(ColSpanProperty);
		set => SetValue(ColSpanProperty, value);
	}

	public static readonly DependencyProperty ColSpanProperty = DependencyProperty.Register(
		nameof(ColSpan),
		typeof(int),
		typeof(PostCardControl),
		new PropertyMetadata(0, OnColSpanChanged)
	);

	private static void OnColSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostCardControl self) {
			self.Raise(nameof(ColSpan));
		}
	}

	public int RowSpan {
		get => (int)GetValue(RowSpanProperty);
		set => SetValue(RowSpanProperty, value);
	}

	public static readonly DependencyProperty RowSpanProperty = DependencyProperty.Register(
		nameof(RowSpan),
		typeof(int),
		typeof(PostCardControl),
		new PropertyMetadata(0, OnRowSpanChanged)
	);

	private static void OnRowSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostCardControl self) {
			self.Raise(nameof(RowSpan));
		}
	}

	public PostImageLoader ImageLoader { get; }

	private readonly PostsViewModel parentViewModel;
	private readonly IViewConfigService viewConfigService;

	public PostCardControl(PostsViewModel parentViewModel, IViewConfigService viewConfigService, E621Post post) {
		this.parentViewModel = parentViewModel;
		this.viewConfigService = viewConfigService;

		Post = post;
		GifAutoPlayType = AppSettingsService.Instance.Model.GifAutoPlayType;

		ImageLoader = new PostImageLoader(post);
		ImageLoader.Progress += ImageLoader_Progress;
		ImageLoader.ImageChanged += ImageLoader_ImageChanged;
		ImageLoader.ImageGifChanged += ImageLoader_ImageGifChanged;

		viewConfigService.PostItemSizeChanged += ViewConfigService_PostItemSizeChanged;

		CalculateSpans();
	}

	public void Dispose() {
		viewConfigService.PostItemSizeChanged -= ViewConfigService_PostItemSizeChanged;
	}

	private void ViewConfigService_PostItemSizeChanged(IViewConfigService sender, EventArgs args) {
		CalculateSpans();
	}

	private void CalculateSpans() {
		Vector2 size = Post.GetSize();
		double ratio = size.X / size.Y;
		double h = viewConfigService.PostItemWidth / ratio / viewConfigService.PostItemHeight;
		int h2 = (int)Math.Ceiling(h);

		ColSpan = 1;
		RowSpan = h2;
	}

	private Border? RootBorder;
	private Storyboard? ScaleOn;
	private Storyboard? ScaleOff;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		RootBorder = (Border)GetTemplateChild(nameof(RootBorder));
		ScaleOn = (Storyboard)FindResource(nameof(ScaleOn));
		ScaleOff = (Storyboard)FindResource(nameof(ScaleOff));

		ImageLoader.Initialize();

	}

	private void ImageLoader_Progress(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!CheckAccess()) {
			Dispatcher.Invoke(ImageLoader_Progress, sender, args);
			return;
		}
		if (args.HasError) {
			LoadingStatus = PostLoadingStatus.HasError;
		} else if (args.HasCompleted) {
			LoadingStatus = PostLoadingStatus.HasCompleted;
		} else if (!args.HasStarted) {
			LoadingStatus = PostLoadingStatus.NotStarted;
		} else {
			LoadingStatus = PostLoadingStatus.Loading;
			LoadingProgress = NumberHelper.Remap(args.Progress, 0, 100, 5, 95);
		}
	}

	private void ImageLoader_ImageChanged(PostImageLoader sender, BitmapImage? args) {
		GifImage = null;
		BitmapImage = args;
	}

	private void ImageLoader_ImageGifChanged(PostImageLoader sender, GifImage? args) {
		BitmapImage = null;
		GifImage = args;
	}

	protected override void OnMouseEnter(MouseEventArgs e) {
		base.OnMouseEnter(e);
		if (parentViewModel.IsMultiSelecting) {
			return;
		}
		ScaleOn?.Begin(RootBorder);
	}

	protected override void OnMouseLeave(MouseEventArgs e) {
		base.OnMouseLeave(e);
		if (parentViewModel.IsMultiSelecting) {
			return;
		}
		ScaleOff?.Begin(RootBorder);
	}

}

public enum PostLoadingStatus {
	NotStarted,
	HasError,
	HasCompleted,
	Loading,
}
