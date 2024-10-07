using BaseFramework.Controls;
using BaseFramework.Enums;
using BaseFramework.Helpers;
using BaseFramework.Interfaces;
using BaseFramework.Models;
using BaseFramework.Services;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;
using YB.E621.Models;
using YB.E621.Models.E621;
using YB.E621.Services;
using YB.E621.Views;

namespace YB.E621.Controls {
	public class PostCardControl : ContentControl, IVariableSizedGridItem {

		public bool IsSelected {
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected),
			typeof(bool),
			typeof(PostCardControl),
			new PropertyMetadata(false)
		);

		public E621Post Post {
			get => (E621Post)GetValue(PostProperty.DependencyProperty);
			init => SetValue(PostProperty, value);
		}

		public static readonly DependencyPropertyKey PostProperty = DependencyProperty.RegisterReadOnly(
			nameof(Post),
			typeof(E621Post),
			typeof(PostCardControl),
			new PropertyMetadata(null)
		);


		public string TypeHint {
			get => (string)GetValue(TypeHintProperty);
			private set => SetValue(TypeHintPropertyKey, value);
		}

		public static readonly DependencyPropertyKey TypeHintPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(TypeHint),
			typeof(string),
			typeof(PostCardControl),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty TypeHintProperty = TypeHintPropertyKey.DependencyProperty;



		public LoadingStatus LoadingStatus {
			get => (LoadingStatus)GetValue(LoadingStatusProperty);
			private set => SetValue(LoadingStatusPropertyKey, value);
		}

		public static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(LoadingStatus),
			typeof(LoadingStatus),
			typeof(PostCardControl),
			new PropertyMetadata(LoadingStatus.NotStarted)
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



		public int ColSpan { get; }
		public int RowSpan { get; }

		public PostImageLoader ImageLoader { get; }

		private static readonly string[] sourceArray = ["gif", "webm", "swf"];

		public PostCardControl(E621Post post) {
			Post = post;

			if (post.File != null && post.File.Ext != null) {
				if (sourceArray.Contains(post.File.Ext.ToLower())) {
					TypeHint = post.File.Ext.ToUpper();
				}
			}

			ImageLoader = new PostImageLoader(post);
			ImageLoader.Progress += ImageLoader_Progress;
			ImageLoader.ImageChanged += ImageLoader_ImageChanged;
			ImageLoader.ImageGifChanged += ImageLoader_ImageGifChanged;

			Vector2 size = post.GetSize();
			double ratio = size.X / size.Y;
			double h = (PostsViewModel.ItemWidth / ratio) / PostsViewModel.ItemHeight;
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

		private void ImageLoader_Progress(BitmapCacheItem sender, BitmapLoadingModel args) {
			if (!CheckAccess()) {
				Dispatcher.Invoke(ImageLoader_Progress, sender, args);
				return;
			}
			if (args.HasError) {
				LoadingStatus = LoadingStatus.HasError;
			} else if (args.HasCompleted) {
				LoadingStatus = LoadingStatus.HasCompleted;
			} else if (!args.HasStarted) {
				LoadingStatus = LoadingStatus.NotStarted;
			} else {
				LoadingStatus = LoadingStatus.Loading;
				LoadingProgress = CommonHelper.Remap(args.Progress, 0, 100, 5, 95);
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
			ScaleOn!.Begin(RootBorder);
		}

		protected override void OnMouseLeave(MouseEventArgs e) {
			base.OnMouseLeave(e);
			ScaleOff!.Begin(RootBorder);
		}

	}

	public enum LoadingStatus {
		NotStarted,
		HasError,
		HasCompleted,
		Loading,
	}
}
