using BaseFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XamlAnimatedGif;

namespace BaseFramework.Controls {
	public class ImageExtend : Image {


		public BitmapImage BitmapImage {
			get => (BitmapImage)GetValue(BitmapImageProperty);
			set => SetValue(BitmapImageProperty, value);
		}

		public static readonly DependencyProperty BitmapImageProperty = DependencyProperty.Register(
			nameof(BitmapImage),
			typeof(BitmapImage),
			typeof(ImageExtend),
			new PropertyMetadata(null, OnBitmapImageChanged, BitmapImageCoerce)
		);

		private static void OnBitmapImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue is BitmapImage bitmap) {
				((ImageExtend)d).LoadBitmap(bitmap);
			}
		}


		private static object BitmapImageCoerce(DependencyObject d, object baseValue) {
			return baseValue;
		}



		public GifImage GifImage {
			get => (GifImage)GetValue(GifImageProperty);
			set => SetValue(GifImageProperty, value);
		}

		public static readonly DependencyProperty GifImageProperty = DependencyProperty.Register(
			nameof(GifImage),
			typeof(GifImage),
			typeof(ImageExtend),
			new PropertyMetadata(null, OnGifImageChanged, GifImageCoerce)
		);

		private static void OnGifImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (e.NewValue is GifImage gif) {
				((ImageExtend)d).LoadGif(gif);
			}
		}

		private static object GifImageCoerce(DependencyObject d, object baseValue) {
			if (baseValue == d.GetValue(GifImageProperty)) {

			}
			return baseValue;
		}


		private void LoadGif(GifImage gifImage) {
			ClearValue(AnimationBehavior.SourceStreamProperty);
			MemoryStream? stream = gifImage.GetMemoryStream();
			if (stream != null) {
				AnimationBehavior.SetSourceStream(this, stream);
				//AnimationBehavior.SetCacheFramesInMemory(this, false);
			}
		}

		private void LoadBitmap(BitmapImage bitmapImage) {
			ClearValue(AnimationBehavior.SourceStreamProperty);
			Source = bitmapImage;
		}

		public void Clear() {
			ClearValue(AnimationBehavior.SourceStreamProperty);
			Source = null;
		}

		private DispatcherTimer ResizeTimer { get; } = new() {
			Interval = TimeSpan.FromMilliseconds(200),
		};

		private bool HasLoaded { get; set; } = false;

		public ImageExtend() {
			ResizeTimer.Tick += ResizeTimer_Tick;
			Loaded += ImageExtend_Loaded;
			Unloaded += ImageExtend_Unloaded;
			IsVisibleChanged += ImageExtend_IsVisibleChanged;

			//AnimationBehavior.SetCacheFramesInMemory(this, true);
		}

		private void ImageExtend_SizeChanged(object sender, SizeChangedEventArgs e) {
			Animator animator = AnimationBehavior.GetAnimator(this);
			if (animator is null) {
				return;
			}
			ResizeTimer.Stop();
			ResizeTimer.Start();
			Dispatcher.Invoke(() => {
				AnimationBehavior.SetCacheFramesInMemory(this, true);
			}, DispatcherPriority.Loaded);
		}

		private void ResizeTimer_Tick(object? sender, EventArgs e) {
			ResizeTimer.Stop();
			Dispatcher.Invoke(() => {
				AnimationBehavior.SetCacheFramesInMemory(this, false);
			}, DispatcherPriority.Loaded);
		}


		private void ImageExtend_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {

		}

		private void ImageExtend_Unloaded(object sender, RoutedEventArgs e) {

		}

		private void ImageExtend_Loaded(object sender, RoutedEventArgs e) {
			if (!HasLoaded) {
				Window.GetWindow(this).SizeChanged += ImageExtend_SizeChanged;
			}
			HasLoaded = true;
		}
	}
}
