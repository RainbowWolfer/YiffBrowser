using BaseFramework.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using YB.E621.Models;
using YB.E621.Models.E621;

namespace YB.E621.Views.Subs {
	public partial class ImageDisplayer : UserControl {



		public PostImageLoader PostImageLoader {
			get => (PostImageLoader)GetValue(PostImageLoaderProperty);
			set => SetValue(PostImageLoaderProperty, value);
		}

		public static readonly DependencyProperty PostImageLoaderProperty = DependencyProperty.Register(
			nameof(PostImageLoader),
			typeof(PostImageLoader),
			typeof(ImageDisplayer),
			new PropertyMetadata(null)
		);




		public E621Post Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(ImageDisplayer),
			new PropertyMetadata(null, OnPostChanged)
		);

		private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is ImageDisplayer view && e.NewValue is E621Post post) {
				//BitmapCacheItem item = BitmapCacheService.Get(post.File.URL);
				//view.Image.Source = new BitmapImage(new Uri(post.File.URL));
				view.ImageViewer.ImageSource = new BitmapImage(new Uri(post.File.URL));
			}
		}

		public ImageDisplayer() {
			InitializeComponent();
		}
	}
}
