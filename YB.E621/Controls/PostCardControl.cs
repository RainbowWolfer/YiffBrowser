using BaseFramework.Helpers;
using BaseFramework.Interfaces;
using BaseFramework.Models;
using BaseFramework.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using YB.E621.Models;
using YB.E621.Models.E621;
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


		public int ColSpan { get; }
		public int RowSpan { get; }

		public PostImageLoader ImageLoader { get; }

		public PostCardControl(E621Post post) {
			Post = post ?? throw new ArgumentNullException(nameof(post));
			ImageLoader = new PostImageLoader(post);
			ImageLoader.Progress += ImageLoader_Progress;
			ImageLoader.ImageChanged += ImageLoader_ImageChanged;

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
		private ImageBrush? ImageBrush;
		private ProgressBar? LoadingProgressBar;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			ScaleOn = (Storyboard)FindResource(nameof(ScaleOn));
			ScaleOff = (Storyboard)FindResource(nameof(ScaleOff));

			RootBorder = (Border)GetTemplateChild(nameof(RootBorder));
			ImageBrush = (ImageBrush)GetTemplateChild(nameof(ImageBrush));
			LoadingProgressBar = (ProgressBar)GetTemplateChild(nameof(LoadingProgressBar));

			ImageLoader.Initialize();

		}

		private void ImageLoader_Progress(BitmapCacheItem sender, BitmapLoadingModel args) {
			if (args.HasError) {
				LoadingProgressBar!.Visibility = Visibility.Collapsed;
			} else if (args.HasCompleted) {
				LoadingProgressBar!.Visibility = Visibility.Collapsed;
			} else if (args.HasStarted) {
				LoadingProgressBar!.Visibility = Visibility.Visible;
				LoadingProgressBar!.IsIndeterminate = true;
			} else {
				LoadingProgressBar!.Visibility = Visibility.Visible;
				LoadingProgressBar!.IsIndeterminate = false;
				LoadingProgressBar!.Value = args.Progress;
			}
		}

		private void ImageLoader_ImageChanged(PostImageLoader sender, BitmapImage? args) {
			ImageBrush!.ImageSource = args;
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
}
