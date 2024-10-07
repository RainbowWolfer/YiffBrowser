using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BaseFramework.Controls {
	public class GifImageControl : Image {
		private bool _isInitialized;
		private GifBitmapDecoder _gifDecoder;

		private int Index { get; set; } = 0;

		private DispatcherTimer IndexTimer { get; } = new() {
			Interval = TimeSpan.FromMilliseconds(1000),
		};

		WriteableBitmap baseImage;

		public GifImageControl() {
			IndexTimer.Tick += IndexTimer_Tick;
		}

		private void IndexTimer_Tick(object? sender, EventArgs e) {
			if (Index + 1 > _gifDecoder.Frames.Count) {
				Index = 0;
			}
			//BitmapFrame frame = _gifDecoder.Frames[Index++];
			//baseImage.Lock();
			//baseImage.WritePixels(new Int32Rect(0, 0, frame.PixelWidth, frame.PixelHeight), frame.CopyPixels(), frame.PixelWidth * ((frame.Format.BitsPerPixel + 7) / 8), 0);
			//baseImage.Unlock();

			BitmapFrame frame = _gifDecoder.Frames[Index++];
			int stride = frame.PixelWidth * ((frame.Format.BitsPerPixel + 7) / 8);
			byte[] pixels = new byte[frame.PixelHeight * stride];
			frame.CopyPixels(pixels, stride, 0);

			baseImage.Lock();
			baseImage.WritePixels(new Int32Rect(0, 0, frame.PixelWidth, frame.PixelHeight), pixels, stride, 0);
			baseImage.Unlock();

			//currentFrame = (currentFrame + 1) % frameCount;
			//Source = _gifDecoder.Frames[Index++];
		}

		private void Initialize() {
			_gifDecoder = new GifBitmapDecoder(new Uri(GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

			TimeSpan span = new TimeSpan(
				days: 0,
				hours: 0,
				minutes: 0,
				seconds: 4,
				milliseconds: 0
			);

			baseImage = new WriteableBitmap(_gifDecoder.Frames[0]);

			//TimeSpan span = new TimeSpan(
			//	days: 0,
			//	hours: 0,
			//	minutes: 0,
			//	seconds: _gifDecoder.Frames.Count / 10,
			//	milliseconds: (int)(((_gifDecoder.Frames.Count / 10.0) - _gifDecoder.Frames.Count / 10) * 1000)
			//);

			//_animation = new Int32Animation(
			//	fromValue: 0,
			//	toValue: _gifDecoder.Frames.Count - 1,
			//	duration: new Duration(span)
			//);
			//_animation.RepeatBehavior = RepeatBehavior.Forever;
			Index = 0;
			this.Source = baseImage;

			_isInitialized = true;
		}

		static GifImageControl() {

		}

		//public static readonly DependencyProperty FrameIndexProperty =
		//	DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImageControl), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

		//private static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
		//	var GifImageControl = obj as GifImageControl;
		//	GifImageControl.Source = GifImageControl._gifDecoder.Frames[(int)args.NewValue];
		//}

		/// <summary>
		/// Defines whether the animation starts on it's own
		/// </summary>
		public bool AutoStart {
			get { return (bool)GetValue(AutoStartProperty); }
			set { SetValue(AutoStartProperty, value); }
		}

		public static readonly DependencyProperty AutoStartProperty =
			DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImageControl), new UIPropertyMetadata(false, AutoStartPropertyChanged));

		private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			if ((bool)e.NewValue) {
				(sender as GifImageControl).StartAnimation();
			}
		}

		public string GifSource {
			get { return (string)GetValue(GifSourceProperty); }
			set { SetValue(GifSourceProperty, value); }
		}

		public static readonly DependencyProperty GifSourceProperty =
			DependencyProperty.Register("GifSource", typeof(string), typeof(GifImageControl), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

		private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			(sender as GifImageControl).Initialize();
		}

		/// <summary>
		/// Starts the animation
		/// </summary>
		public void StartAnimation() {
			if (!_isInitialized) {
				this.Initialize();
			}
			IndexTimer.Start();
			//BeginAnimation(FrameIndexProperty, _animation);
		}

		/// <summary>
		/// Stops the animation
		/// </summary>
		public void StopAnimation() {
			IndexTimer.Stop();
			//BeginAnimation(FrameIndexProperty, null);
		}
	}
}
