using YiffBrowser.BaseFramework.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace YiffBrowser.BaseFramework.Controls;

public class ImageExtend : Image {

	static ImageExtend() {
		VisibilityProperty.OverrideMetadata(
			typeof(ImageExtend),
			new FrameworkPropertyMetadata(
				AppConfig.SafeMode ? Visibility.Hidden : Visibility.Visible,
				null,
				CoerceVisibility));
	}

	private static object CoerceVisibility(DependencyObject d, object baseValue) {
		return AppConfig.SafeMode ? Visibility.Hidden : baseValue;
	}

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
		ImageExtend control = (ImageExtend)d;
		if (e.NewValue is BitmapImage bitmap) {
			control.LoadBitmap(bitmap);
		} else if (e.OldValue != null && control.GifImage == null) {
			control.Clear();
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
		ImageExtend control = (ImageExtend)d;
		if (e.NewValue is GifImage gif) {
			control.LoadGif(gif);
		} else if (e.OldValue != null && control.BitmapImage == null) {
			control.Clear();
		}
	}

	private static object GifImageCoerce(DependencyObject d, object baseValue) {
		return baseValue;
	}


	private void LoadGif(GifImage gifImage) {
		ClearValue(AnimationBehavior.SourceStreamProperty);
		Source = null;
		MemoryStream? stream = gifImage.GetMemoryStream();
		if (stream != null) {
			AnimationBehavior.SetSourceStream(this, stream);
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
