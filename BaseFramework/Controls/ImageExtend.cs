using BaseFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
			return baseValue;
		}


		private void LoadGif(GifImage gifImage) {
			if (gifImage?.MemoryStream != null) {
				AnimationBehavior.SetSourceStream(this, gifImage.MemoryStream);
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

	}
}
